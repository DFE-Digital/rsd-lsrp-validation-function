namespace GovUK.Dfe.Lsrp.FileValidator.Models;

public class SpreadsheetMap
{
    public string? Worksheet { get; set; }
    public IEnumerable<DataMap>? DataMaps { get; set; }
}
