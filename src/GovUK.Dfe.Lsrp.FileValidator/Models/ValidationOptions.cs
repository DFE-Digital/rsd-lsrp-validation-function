using RulesEngine.Models;

namespace GovUK.Dfe.Lsrp.FileValidator.Models;

public class ValidationOptions
{
    public IEnumerable<SpreadsheetMap>? SpreadsheetMaps { get; set; }
    public IEnumerable<Workflow>? Workflows { get; set; }
}