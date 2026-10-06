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
        validationService.ValidateAsync(Arg.Any<MessageData>(), Arg.Any<List<string>>()).Returns(Task.FromResult(true));

        IFileValidationResultService validationResultService = Substitute.For<IFileValidationResultService>();
        validationResultService.SendResultAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<IEnumerable<string>>()).Returns(Task.CompletedTask);
        SpreadsheetValidatorFunction function = new(validationService, validationResultService, NullLogger<SpreadsheetValidatorFunction>.Instance);
        ServiceBusReceivedMessage message = CreateMessage("test.xlsx", "file-id-123", new LocalAuthority { Code = "LA-Code", Name = "LA-Name" });
        ServiceBusMessageActions messageActions = Substitute.For<ServiceBusMessageActions>();

        await function.Run(message, messageActions);

        await validationService.Received(1).ValidateAsync(Arg.Any<MessageData>(), Arg.Any<List<string>>());
        await validationResultService.Received(1).SendResultAsync(Arg.Any<string>(), true, Arg.Any<IEnumerable<string>>());
        await messageActions.Received(1).CompleteMessageAsync(message, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Run_WhenMessageIsNotValid_ThrowsInvalidDataException()
    {
        ISpreadsheetValidationService validationService = Substitute.For<ISpreadsheetValidationService>();
        IFileValidationResultService validationResultService = Substitute.For<IFileValidationResultService>();
        SpreadsheetValidatorFunction function = new(validationService, validationResultService, NullLogger<SpreadsheetValidatorFunction>.Instance);
        ServiceBusReceivedMessage message = CreateMessage(null, null);
        ServiceBusMessageActions messageActions = Substitute.For<ServiceBusMessageActions>();

        await Assert.ThrowsAsync<InvalidDataException>(() => function.Run(message, messageActions));

        await validationService.DidNotReceive().ValidateAsync(Arg.Any<MessageData>(), Arg.Any<List<string>>());
        await validationResultService.DidNotReceive().SendResultAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<IEnumerable<string>>());
        await messageActions.DidNotReceive().CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());
    }

    private static ServiceBusReceivedMessage CreateMessage(string? fileUri, string? fileId, LocalAuthority? localAuthority = null)
    {
        Payload payload = new()
        {
            FileUri = fileUri,
            FileId = fileId,
            FileName = "file.xlsx"
        };
        if (localAuthority != null)
        {
            payload.LocalAuthority = JsonSerializer.Serialize(localAuthority);
        }

        var message = new FileUploadedMessage
        {
            Message = new Message
            {
                Metadata = new Metadata
                {
                    ApplicationId = "00000000-0000-0000-0000-000000000001",
                    ApplicationReference = "APP-001"
                },
                Payload = payload
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
        IFileValidationResultService validationResultService = Substitute.For<IFileValidationResultService>();
        SpreadsheetValidatorFunction function = new(validationService, validationResultService, NullLogger<SpreadsheetValidatorFunction>.Instance);
        ServiceBusReceivedMessage message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            messageId: Guid.Empty.ToString(),
            contentType: "application/json");
        ServiceBusMessageActions messageActions = Substitute.For<ServiceBusMessageActions>();

        await Assert.ThrowsAsync<JsonException>(() => function.Run(message, messageActions));

        await validationService.DidNotReceive().ValidateAsync(Arg.Any<MessageData>(), Arg.Any<List<string>>());
        await validationResultService.DidNotReceive().SendResultAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<IEnumerable<string>>());
        await messageActions.DidNotReceive().CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Run_WhenLocalAuthorityMissing_ThrowsValidationError()
    {
        ISpreadsheetValidationService validationService = Substitute.For<ISpreadsheetValidationService>();
        IFileValidationResultService validationResultService = Substitute.For<IFileValidationResultService>();
        SpreadsheetValidatorFunction function = new(validationService, validationResultService, NullLogger<SpreadsheetValidatorFunction>.Instance);
        ServiceBusReceivedMessage message = CreateMessage("test.xlsx", "file-id-123", null);
        ServiceBusMessageActions messageActions = Substitute.For<ServiceBusMessageActions>();
        await function.Run(message, messageActions);
        await validationService.DidNotReceive().ValidateAsync(Arg.Any<MessageData>(), Arg.Any<List<string>>());
        await validationResultService.Received(1).SendResultAsync("file-id-123", false, Arg.Is<IEnumerable<string>>(errors => new List<string>(errors).Contains("Local authority is missing or invalid.")));
        await messageActions.Received().CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());
    }
}
