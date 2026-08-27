using Azure.Messaging.ServiceBus;
using GovUK.Dfe.Lsrp.FileValidator.Models;
using GovUK.Dfe.Lsrp.FileValidator.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GovUK.Dfe.Lsrp.FileValidator;

public class SpreadsheetValidatorFunction(IMessageParser messageParser, ISpreadsheetValidationService validationService, ILogger<SpreadsheetValidatorFunction> logger)
{
    private readonly JsonSerializerOptions jsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Function(nameof(SpreadsheetValidatorFunction))]
    public async Task Run([ServiceBusTrigger("%Topic%", "%Subscription%")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
    {
        logger.LogInformation("Message ID: {id}", message.MessageId);
        logger.LogInformation("Message Body Length: {length}", message.Body.ToMemory().Length);
        logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        FileUploadedMessage? fileMessage = JsonSerializer.Deserialize<FileUploadedMessage>(message.Body.ToString(), jsonOptions) ?? throw new ArgumentException("Message body is empty or not valid JSON.");

        if (!messageParser.Parse(fileMessage)) throw new InvalidDataException("Message body not valid");

        User user = new() { LocalAuthority = messageParser.LocalAuthority! };

        List<string> errors = [];
        bool isValid = await validationService.ValidateAsync(user, messageParser.FileUri!, errors);
        logger.LogInformation("Spreadsheet validation {result} for message ID {messageId}. {errors}", isValid ? "succeeded" : "failed", messageParser.MessageId, string.Join(", ", errors));

        // TODO log validation result to database Files table via new API endpoint

        await messageActions.CompleteMessageAsync(message);
    }
}