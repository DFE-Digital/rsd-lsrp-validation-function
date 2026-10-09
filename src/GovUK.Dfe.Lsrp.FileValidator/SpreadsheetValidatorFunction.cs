using Azure.Messaging.ServiceBus;
using GovUK.Dfe.Lsrp.FileValidator.Models;
using GovUK.Dfe.Lsrp.FileValidator.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GovUK.Dfe.Lsrp.FileValidator;

public class SpreadsheetValidatorFunction(
    IMessageParser messageParser,
    ISpreadsheetValidationService validationService, 
    IFileValidationResultService validationResultService, 
    ILogger<SpreadsheetValidatorFunction> logger)
{
    private readonly JsonSerializerOptions jsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Function(nameof(SpreadsheetValidatorFunction))]
    public async Task Run([ServiceBusTrigger("%Topic%", "%Subscription%")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
    {
        logger.LogInformation("Message ID: {id}. Body Length: {length}. Content-Type: {contentType}", message.MessageId, message.Body.ToMemory().Length, message.ContentType);

        FileUploadedMessage? fileMessage = JsonSerializer.Deserialize<FileUploadedMessage>(message.Body.ToString(), jsonOptions) ?? throw new ArgumentException("Message body is empty or not valid JSON.");

        List<string> errors = [];

        if (!messageParser.Parse(fileMessage, out MessageData? messageData, errors))
        {
            throw new InvalidDataException($"Message body not valid: {string.Join(", ", errors)}");
        }

        bool isValid = errors.Count == 0 && await validationService.ValidateAsync(messageData!, errors);

        logger.LogInformation("Spreadsheet validation {result} for message ID {messageId}. {errors}", isValid ? "succeeded" : "failed", messageData?.MessageId, string.Join(", ", errors));

        if (string.IsNullOrEmpty(messageData?.FileId))
        {
            throw new InvalidDataException($"Message has no FileId: {string.Join(", ", errors)}");
        }

        await validationResultService.SendResultAsync(messageData.FileId, isValid, errors);

        await messageActions.CompleteMessageAsync(message);
    }
}