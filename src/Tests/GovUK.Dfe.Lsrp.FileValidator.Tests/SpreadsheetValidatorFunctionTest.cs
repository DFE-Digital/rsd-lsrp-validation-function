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
        validationService.ValidateAsync(Arg.Any<string>(), Arg.Any<List<string>>()).Returns(Task.FromResult(true));

        SpreadsheetValidatorFunction function = new(validationService, NullLogger<SpreadsheetValidatorFunction>.Instance);
        ServiceBusReceivedMessage message = CreateMessage(new FileMessage
        {
            ApplicationId = Guid.Empty,
            ApplicationReference = "APP-001",
            Filename = "test.xlsx",
            UserEmail = "user@example.com"
        });
        ServiceBusMessageActions messageActions = Substitute.For<ServiceBusMessageActions>();

        await function.Run(message, messageActions);

        await validationService.Received(1).ValidateAsync("test.xlsx", Arg.Any<List<string>>());
        await messageActions.Received(1).CompleteMessageAsync(message, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Run_WhenMessageIsNotValid_ThrowsInvalidDataException()
    {
        ISpreadsheetValidationService validationService = Substitute.For<ISpreadsheetValidationService>();
        SpreadsheetValidatorFunction function = new(validationService, NullLogger<SpreadsheetValidatorFunction>.Instance);
        ServiceBusReceivedMessage message = CreateMessage(new FileMessage
        {
            ApplicationId = Guid.Empty,
            ApplicationReference = "APP-001",
            Filename = "",
            UserEmail = "user@example.com"
        });
        ServiceBusMessageActions messageActions = Substitute.For<ServiceBusMessageActions>();

        await Assert.ThrowsAsync<InvalidDataException>(() => function.Run(message, messageActions));

        await validationService.DidNotReceive().ValidateAsync(Arg.Any<string>(), Arg.Any<List<string>>());
        await messageActions.DidNotReceive().CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Run_WhenJsonRepresentsNull_ThrowsArgumentException()
    {
        ISpreadsheetValidationService validationService = Substitute.For<ISpreadsheetValidationService>();
        SpreadsheetValidatorFunction function = new(validationService, NullLogger<SpreadsheetValidatorFunction>.Instance);
        ServiceBusReceivedMessage message = CreateMessageFromJson("null");
        ServiceBusMessageActions messageActions = Substitute.For<ServiceBusMessageActions>();

        await Assert.ThrowsAsync<ArgumentException>(() => function.Run(message, messageActions));

        await validationService.DidNotReceive().ValidateAsync(Arg.Any<string>(), Arg.Any<List<string>>());
        await messageActions.DidNotReceive().CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());
    }

    private static ServiceBusReceivedMessage CreateMessage(FileMessage payload)
        => CreateMessageFromJson(JsonSerializer.Serialize(payload));

    private static ServiceBusReceivedMessage CreateMessageFromJson(string json)
        => ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: BinaryData.FromString(json),
            messageId: Guid.Empty.ToString(),
            contentType: "application/json");
}
