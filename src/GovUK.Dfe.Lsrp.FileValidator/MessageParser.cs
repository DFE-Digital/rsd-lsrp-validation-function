using GovUK.Dfe.Lsrp.FileValidator.Models;
using System.Text.Json;

namespace GovUK.Dfe.Lsrp.FileValidator
{
    public class MessageParser
    {
        private static readonly JsonSerializerOptions? jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public static bool Parse(FileUploadedMessage fileMessage, out MessageData? messageData)
        {
            if (!ValidateMessage(fileMessage))
            {
                messageData = null;
                return false;
            }

            messageData = new MessageData
            {
                FileUri = fileMessage.Message?.Payload?.FileUri,
                FileId = fileMessage.Message?.Payload?.FileId,
                MessageId = fileMessage.MessageId,
                ApplicationId = fileMessage.Message?.Metadata?.ApplicationId,
                LocalAuthority = JsonSerializer.Deserialize<LocalAuthority>(fileMessage.Message?.Payload?.LocalAuthority!, jsonOptions)
            };

            return true;
        }

        private static bool ValidateMessage(FileUploadedMessage fileMessage) => fileMessage.Message != null && HasFile(fileMessage.Message) && HasApplication(fileMessage.Message);
        private static bool HasFile(Message? message) => message != null && message.Payload != null && !string.IsNullOrEmpty(message.Payload.FileUri) && !string.IsNullOrEmpty(message.Payload.FileId);
        private static bool HasApplication(Message? message) => message != null && message.Metadata != null && !string.IsNullOrEmpty(message.Metadata.ApplicationId) && !string.IsNullOrEmpty(message.Metadata.ApplicationReference);
    }
}