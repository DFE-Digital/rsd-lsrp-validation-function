using Azure.Messaging.ServiceBus;
using GovUK.Dfe.Lsrp.FileValidator.Models;
using GovUK.Dfe.Lsrp.FileValidator.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.Text.Json;

namespace GovUK.Dfe.Lsrp.FileValidator.Tests;

public class SpreadsheetValidatorFunctionTest
{
    [Fact]
    public async Task Run_WhenMessageIsValid_CallsValidationAndCompletesMessage()
    {
        ISpreadsheetValidationService validationService = Substitute.For<ISpreadsheetValidationService>();
        validationService.ValidateAsync(Arg.Any<User>(), "test.xlsx", Arg.Any<List<string>>()).Returns(Task.FromResult(true));

        IMessageParser messageParser = new MessageParser();
        IFileValidationResultService validationResultService = Substitute.For<IFileValidationResultService>();
        validationResultService.SendResultAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<IEnumerable<string>>()).Returns(Task.CompletedTask);
        SpreadsheetValidatorFunction function = new(messageParser, validationService, validationResultService, NullLogger<SpreadsheetValidatorFunction>.Instance);
        ServiceBusReceivedMessage message = CreateMessage("test.xlsx");
        ServiceBusMessageActions messageActions = Substitute.For<ServiceBusMessageActions>();

        await function.Run(message, messageActions);

        await validationService.Received(1).ValidateAsync(Arg.Any<User>(), "test.xlsx", Arg.Any<List<string>>());
        await messageActions.Received(1).CompleteMessageAsync(message, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Run_WhenMessageIsNotValid_ThrowsInvalidDataException()
    {
        ISpreadsheetValidationService validationService = Substitute.For<ISpreadsheetValidationService>();
        IMessageParser messageParser = new MessageParser();
        IFileValidationResultService validationResultService = Substitute.For<IFileValidationResultService>();
        SpreadsheetValidatorFunction function = new(messageParser, validationService, validationResultService, NullLogger<SpreadsheetValidatorFunction>.Instance);
        ServiceBusReceivedMessage message = CreateMessage(null);
        ServiceBusMessageActions messageActions = Substitute.For<ServiceBusMessageActions>();

        await Assert.ThrowsAsync<InvalidDataException>(() => function.Run(message, messageActions));

        await validationService.DidNotReceive().ValidateAsync(Arg.Any<User>(), "test.xlsx", Arg.Any<List<string>>());
        await messageActions.DidNotReceive().CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());
    }

    private static ServiceBusReceivedMessage CreateMessage(string? fileUri)
    {
        var message = new FileUploadedMessage
        {
            Message = new Message
            {
                Metadata = new Metadata
                {
                    ApplicationId = "00000000-0000-0000-0000-000000000001",
                    ApplicationReference = "APP-001",
                    LocalAuthority = "LA1"
                },
                Payload = new Payload
                {
                    FileUri = fileUri
                }
            }
        };
        return ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: BinaryData.FromString(JsonSerializer.Serialize(message)),
            messageId: Guid.Empty.ToString(),
            contentType: "application/json");
    }

    [Fact]
    public async Task Run_WhenJsonRepresentsNull_ThrowsArgumentException()
    {
        ISpreadsheetValidationService validationService = Substitute.For<ISpreadsheetValidationService>();
        IMessageParser messageParser = new MessageParser();
        IFileValidationResultService validationResultService = Substitute.For<IFileValidationResultService>();
        SpreadsheetValidatorFunction function = new(messageParser, validationService, validationResultService, NullLogger<SpreadsheetValidatorFunction>.Instance);
        ServiceBusReceivedMessage message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: BinaryData.FromString(JsonSerializer.Serialize<object>(null)),
            messageId: Guid.Empty.ToString(),
            contentType: "application/json");
        ServiceBusMessageActions messageActions = Substitute.For<ServiceBusMessageActions>();

        await Assert.ThrowsAsync<ArgumentException>(() => function.Run(message, messageActions));

        await validationService.DidNotReceive().ValidateAsync(Arg.Any<User>(), "test.xlsx", Arg.Any<List<string>>());
        await messageActions.DidNotReceive().CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());
    }
}
