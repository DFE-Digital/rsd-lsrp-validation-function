using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUK.Dfe.Lsrp.FileValidator;

public class SpreadsheetValidator(ILogger<SpreadsheetValidator> logger)
{
    [Function(nameof(SpreadsheetValidator))]
    public async Task Run(
        [ServiceBusTrigger("mytopic", "mysubscription", Connection = "")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        logger.LogInformation("Message ID: {id}", message.MessageId);
        logger.LogInformation("Message Body: {body}", message.Body);
        logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

            // Complete the message
        await messageActions.CompleteMessageAsync(message);
    }
}