using GovUK.Dfe.Lsrp.FileValidator.Models;
using Microsoft.Extensions.Options;

namespace GovUK.Dfe.Lsrp.FileValidator.Services;

public class SpreadsheetValidationService(IFileProvider fileProvider, ISpreadsheetDataProvider dataProvider, IDataValidator dataValidator, IOptions<ValidationOptions> options)
    : ISpreadsheetValidationService
{
    public async Task<bool> ValidateAsync(string filename, List<string> errors)
    {
        var spreadsheetMaps = options.Value.SpreadsheetMaps ?? throw new InvalidOperationException("Spreadsheet maps missing in configuration.");
        var workflows = options.Value.Workflows ?? throw new InvalidOperationException("Workflows missing in configuration.");

        using Stream stream = await fileProvider.GetFileAsync(filename) ?? throw new InvalidOperationException("File stream null.");

        object data = await dataProvider.GetDataAsync(stream, spreadsheetMaps, errors) ?? throw new InvalidOperationException("Spreadsheet data null.");
        
        return errors.Count == 0 && await dataValidator.ValidateAsync(data, workflows, errors);
    }
}
