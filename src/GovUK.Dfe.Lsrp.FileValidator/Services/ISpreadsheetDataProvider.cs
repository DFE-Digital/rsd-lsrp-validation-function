using GovUK.Dfe.Lsrp.FileValidator.Models;

namespace GovUK.Dfe.Lsrp.FileValidator.Services;

public interface ISpreadsheetDataProvider
{
    Task<object> GetDataAsync(Stream stream, IEnumerable<SpreadsheetMap> spreadsheetMaps, IList<string> errors);
}
