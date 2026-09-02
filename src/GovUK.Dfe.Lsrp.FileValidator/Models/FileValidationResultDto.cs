namespace GovUK.Dfe.Lsrp.FileValidator.Models;

public class FileValidationResultDto
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public string? Source { get; set; }
}
