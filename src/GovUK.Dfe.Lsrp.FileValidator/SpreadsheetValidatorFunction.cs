using Azure.Messaging.ServiceBus;
using GovUK.Dfe.Lsrp.FileValidator.Models;
using GovUK.Dfe.Lsrp.FileValidator.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GovUK.Dfe.Lsrp.FileValidator;

public class SpreadsheetValidatorFunction(ISpreadsheetValidationService validationService, ILogger<SpreadsheetValidatorFunction> logger)
{
    [Function(nameof(SpreadsheetValidatorFunction))]
    public async Task Run(
        [ServiceBusTrigger("%TopicName%", "%SubscriptionName%")] ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        logger.LogInformation("Message ID: {id}", message.MessageId);
        logger.LogInformation("Message Body Length: {length}", message.Body.ToMemory().Length);
        logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        FileUploadedMessage? fileMessage = JsonSerializer.Deserialize<FileUploadedMessage>(message.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (fileMessage == null) throw new ArgumentException("Message body is empty or not valid JSON.");
        if (!fileMessage.IsValid) throw new InvalidDataException("Message body not valid");

        List<string> errors = [];
        if (await validationService.ValidateAsync(fileMessage.Message!.Payload!.FileUri!, errors))
        {
            logger.LogInformation("Spreadsheet is valid for message ID {messageId}", fileMessage.MessageId);
        }
        else
        {
            logger.LogError("Spreadsheet is not valid for message ID {messageId}: {errors}", fileMessage.MessageId  , string.Join(", ", errors));
        }

        // TODO log validation result to database Files table via new API endpoint

        // TODO email user of validation result (errors)?

        await messageActions.CompleteMessageAsync(message);
    }
}