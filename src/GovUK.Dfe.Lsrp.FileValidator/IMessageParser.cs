using GovUK.Dfe.Lsrp.FileValidator.Models;

namespace GovUK.Dfe.Lsrp.FileValidator
{
    public interface IMessageParser
    {
        string? ApplicationId { get; }
        string? FileUri { get; }
        string? MessageId { get; }
        string? LocalAuthority { get; }

        bool Parse(FileUploadedMessage fileMessage);
    }
}