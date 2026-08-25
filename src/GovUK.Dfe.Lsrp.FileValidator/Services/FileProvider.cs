namespace GovUK.Dfe.Lsrp.FileValidator.Services;

public class FileProvider(IHttpClientFactory httpClientFactory) : IFileProvider
{
    /// <summary>
    /// Retrieves a file from Azure file storage using the specified URI (with SAS key).
    /// </summary>
    public async Task<Stream> GetFileAsync(string fileUri)
    {
        HttpClient httpClient = httpClientFactory.CreateClient();
        using HttpResponseMessage response = await httpClient.GetAsync(fileUri);
        response.EnsureSuccessStatusCode();
        MemoryStream stream = new();
        await response.Content.CopyToAsync(stream);
        stream.Position = 0;
        return stream;
    }
}
