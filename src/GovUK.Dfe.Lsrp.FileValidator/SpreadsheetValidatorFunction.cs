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

        FileMessage fileMessage = JsonSerializer.Deserialize<FileMessage>(message.Body) ?? throw new ArgumentException("Message body not JSON");
        if (!fileMessage.IsValid) throw new InvalidDataException("Message body not valid");

        List<string> errors = [];
        if (await validationService.ValidateAsync(fileMessage.Filename!, errors))
        {
            logger.LogInformation("Spreadsheet {filename} is valid", fileMessage.Filename);
        }
        else
        {
            logger.LogError("Spreadsheet {filename} is not valid: {errors}", fileMessage.Filename, string.Join(", ", errors));
        }

        // TODO log validation result to database Files table via new API endpoint

        // TODO email user of validation result (errors)?

        await messageActions.CompleteMessageAsync(message);
    }
}