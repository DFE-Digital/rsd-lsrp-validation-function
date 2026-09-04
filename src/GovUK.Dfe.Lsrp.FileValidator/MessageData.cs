using GovUK.Dfe.Lsrp.FileValidator.Models;

namespace GovUK.Dfe.Lsrp.FileValidator
{
    public class MessageData
    {
        public string? FileUri { get; set; }
        public string? FileId { get; set; }
        public string? MessageId { get; set; }
        public string? ApplicationId { get; set; }
        public LocalAuthority? LocalAuthority { get; set; }
    }
}