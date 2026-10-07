using GovUK.Dfe.Lsrp.FileValidator.Models;
using System.Text.Json;

namespace GovUK.Dfe.Lsrp.FileValidator
{
    public class MessageParser
    {
        private static readonly JsonSerializerOptions? jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public static bool Parse(FileUploadedMessage fileMessage, out MessageData? messageData, List<string> errors)
        {
            messageData = null;

            if (fileMessage.Message == null)
            {
                errors.Add("Message is null.");
                return false;
            }

            if (!ValidateMessage(fileMessage, errors))
            {
                return false;
            }

            messageData = new MessageData
            {
                FileUri = fileMessage.Message?.Payload?.FileUri,
                FileId = fileMessage.Message?.Payload?.FileId,
                FileName = fileMessage.Message?.Payload?.FileName,
                MessageId = fileMessage.MessageId,
                ApplicationId = fileMessage.Message?.Metadata?.ApplicationId
            };

            if (!HasLocalAuthority(fileMessage.Message, out LocalAuthority? localAuthority))
            {
                errors.Add("Local authority is missing or invalid.");
            }

            messageData.LocalAuthority = localAuthority;

            return true;
        }

        private static bool ValidateMessage(FileUploadedMessage fileMessage, List<string> errors)
        {
            var isValid = true;

            if (!HasFile(fileMessage.Message))
            {
                errors.Add("File information is missing or incomplete.");
                isValid = false;
            }

            if (!HasApplication(fileMessage.Message))
            {
                errors.Add("Application information is missing or incomplete.");
                isValid = false;
            }

            return isValid;
        }

        private static bool HasFile(Message? message) => message != null && message.Payload != null && !string.IsNullOrEmpty(message.Payload.FileUri) && !string.IsNullOrEmpty(message.Payload.FileId) && !string.IsNullOrEmpty(message.Payload.FileName);

        private static bool HasApplication(Message? message) => message != null && message.Metadata != null && !string.IsNullOrEmpty(message.Metadata.ApplicationId) && !string.IsNullOrEmpty(message.Metadata.ApplicationReference);
        
        private static bool HasLocalAuthority(Message? message, out LocalAuthority? localAuthority)
        {
            localAuthority = null;
            if (message == null || message.Payload == null || string.IsNullOrEmpty(message.Payload.LocalAuthority))
            {
                return false;
            }

            try
            {
                localAuthority = JsonSerializer.Deserialize<LocalAuthority>(message.Payload.LocalAuthority, jsonOptions);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}