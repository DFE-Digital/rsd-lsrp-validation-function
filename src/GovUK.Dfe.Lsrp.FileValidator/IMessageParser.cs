using GovUK.Dfe.Lsrp.FileValidator.Models;

namespace GovUK.Dfe.Lsrp.FileValidator
{
    public interface IMessageParser
    {
        bool Parse(FileUploadedMessage fileMessage, out MessageData? messageData, List<string> errors);
    }
}