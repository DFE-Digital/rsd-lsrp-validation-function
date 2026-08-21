using System.Text.Json.Serialization;

namespace GovUK.Dfe.Lsrp.FileValidator.Models;

public class FileMessage
{
    public Guid? ApplicationId { get; set; }
    public string? ApplicationReference { get; set; }
    public string? Filename { get; set; }
    public string? UserEmail { get; set; }

    [JsonIgnore]
    public bool IsValid => ApplicationId.HasValue && !string.IsNullOrEmpty(ApplicationReference) && !string.IsNullOrEmpty(Filename) && !string.IsNullOrEmpty(UserEmail);
}
