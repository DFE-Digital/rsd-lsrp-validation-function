namespace GovUK.Dfe.Lsrp.FileValidator.Services;

public interface IFileProvider
{
    Task<Stream> GetFileAsync(string fileUri);
}
