using GovUK.Dfe.Lsrp.FileValidator.Models;

namespace GovUK.Dfe.Lsrp.FileValidator.Services;

public interface ISpreadsheetDataProvider
{
    IDictionary<string, object?> GetData(Stream stream, IEnumerable<SpreadsheetMap> spreadsheetMaps, IList<string> errors);
}
