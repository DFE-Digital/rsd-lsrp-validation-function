using GovUK.Dfe.Lsrp.FileValidator.Models;

namespace GovUK.Dfe.Lsrp.FileValidator
{
    public class MessageParser : IMessageParser
    {
        public string? FileUri { get; private set; }
        public string? MessageId { get; private set; }
        public string? ApplicationId { get; private set; }

        public bool Parse(FileUploadedMessage fileMessage)
        {
            if (!ValidateMessage(fileMessage)) return false;

            FileUri = fileMessage.Message?.Payload?.FileUri;
            MessageId = fileMessage.MessageId;
            ApplicationId = fileMessage.Message?.Metadata?.ApplicationId;

            return fileMessage.Message != null && HasFile(fileMessage.Message) && HasApplication(fileMessage.Message);
        }

        private static bool ValidateMessage(FileUploadedMessage fileMessage) => fileMessage.Message != null && HasFile(fileMessage.Message) && HasApplication(fileMessage.Message);
        private static bool HasFile(Message? message) => message != null && message.Payload != null && !string.IsNullOrEmpty(message.Payload.FileUri);
        private static bool HasApplication(Message? message) => message != null && message.Metadata != null && !string.IsNullOrEmpty(message.Metadata.ApplicationId) && !string.IsNullOrEmpty(message.Metadata.ApplicationReference);
    }
}