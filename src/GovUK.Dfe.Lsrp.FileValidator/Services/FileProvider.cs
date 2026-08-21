using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.Extensions.Configuration;

namespace GovUK.Dfe.Lsrp.FileValidator.Services;

public class FileProvider(IConfiguration config) : IFileProvider
{
    public async Task<Stream> GetFileAsync(string filename)
    {
        string? connectionString = config["WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"];
        string? shareName = config["WEBSITE_CONTENTSHARE"];
        string? dirName = config["WEBSITE_CONTENTDIRECTORY"];

        if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(shareName) || string.IsNullOrWhiteSpace(dirName))
        {
            throw new InvalidOperationException("Azure File Share configuration is missing (WEBSITE_CONTENTAZUREFILECONNECTIONSTRING/WEBSITE_CONTENTSHARE/WEBSITE_CONTENTDIRECTORY).");
        }

        ShareClient share = new(connectionString, shareName);
        ShareDirectoryClient directory = share.GetDirectoryClient(dirName);
        ShareFileClient file = directory.GetFileClient(filename);

        ShareFileDownloadInfo download = await file.DownloadAsync();
        MemoryStream stream = new();
        await download.Content.CopyToAsync(stream);
        stream.Position = 0;

        return stream;
    }
}
