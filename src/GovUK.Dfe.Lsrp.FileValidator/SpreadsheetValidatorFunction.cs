using Azure.Messaging.ServiceBus;
using GovUK.Dfe.Lsrp.FileValidator.Models;
using GovUK.Dfe.Lsrp.FileValidator.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GovUK.Dfe.Lsrp.FileValidator;

public class SpreadsheetValidatorFunction(
    ISpreadsheetValidationService validationService, 
    IFileValidationResultService validationResultService, 
    ILogger<SpreadsheetValidatorFunction> logger)
{
    private readonly JsonSerializerOptions jsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Function(nameof(SpreadsheetValidatorFunction))]
    public async Task Run([ServiceBusTrigger("%Topic%", "%Subscription%")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
    {
        logger.LogInformation("Message ID: {id}", message.MessageId);
        logger.LogInformation("Message Body Length: {length}", message.Body.ToMemory().Length);
        logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        FileUploadedMessage? fileMessage = JsonSerializer.Deserialize<FileUploadedMessage>(message.Body.ToString(), jsonOptions) ?? throw new ArgumentException("Message body is empty or not valid JSON.");

        if (!MessageParser.Parse(fileMessage, out MessageData? messageData)) throw new InvalidDataException("Message body not valid");

        User user = new() { LocalAuthority = messageData!.LocalAuthority!.ToString() };

        List<string> errors = [];
        bool isValid = await validationService.ValidateAsync(user, messageData.FileUri!, errors);
        logger.LogInformation("Spreadsheet validation {result} for message ID {messageId}. {errors}", isValid ? "succeeded" : "failed", messageData.MessageId, string.Join(", ", errors));

        await validationResultService.SendResultAsync(messageData.FileId!, isValid, errors);

        await messageActions.CompleteMessageAsync(message);
    }
}