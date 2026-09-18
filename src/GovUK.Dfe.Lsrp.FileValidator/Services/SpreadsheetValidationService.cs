using GovUK.Dfe.Lsrp.FileValidator.Models;
using Microsoft.Extensions.Options;

namespace GovUK.Dfe.Lsrp.FileValidator.Services;

public class SpreadsheetValidationService(IFilenameValidator filenameValidator, IFileProvider fileProvider, ISpreadsheetDataProvider dataProvider, IDataValidator dataValidator, IOptions<ValidationOptions> options)
    : ISpreadsheetValidationService
{
    public async Task<bool> ValidateAsync(MessageData messageData, List<string> errors)
    {
        var spreadsheetMaps = options.Value.SpreadsheetMaps ?? throw new InvalidOperationException("Spreadsheet maps missing in configuration.");
        var workflows = options.Value.Workflows ?? throw new InvalidOperationException("Workflows missing in configuration.");

        var filenameValid = await filenameValidator.ValidateFilenameAsync(messageData.FileName!, messageData.LocalAuthority!, errors);
        if (!filenameValid)
        {
            return false;
        }

        using Stream stream = await fileProvider.GetFileAsync(messageData.FileUri!) ?? throw new InvalidOperationException("File stream null.");

        var data = dataProvider.GetData(stream, spreadsheetMaps, errors) ?? throw new InvalidOperationException("Spreadsheet data null.");

        return errors.Count == 0 && await dataValidator.ValidateAsync(data, messageData.LocalAuthority!, workflows, errors);
    }
}
