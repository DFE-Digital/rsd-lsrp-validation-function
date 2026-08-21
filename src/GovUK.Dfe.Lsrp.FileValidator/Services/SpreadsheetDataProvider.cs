using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using GovUK.Dfe.Lsrp.FileValidator.Models;
using System.Dynamic;

namespace GovUK.Dfe.Lsrp.FileValidator.Services;

public class SpreadsheetDataProvider : ISpreadsheetDataProvider
{
    public Task<object> GetDataAsync(Stream stream, IEnumerable<SpreadsheetMap> spreadsheetMaps, IList<string> errors)
    {
        using SpreadsheetDocument document = SpreadsheetDocument.Open(stream, false);
        WorkbookPart? wbPart = document.WorkbookPart;
        if (wbPart == null)
        {
            errors.Add("WorkbookPart is null.");
            return Task.FromResult((object)new ExpandoObject());
        }

        var data = new ExpandoObject() as IDictionary<string, object?>;
        foreach (var spreadsheetMap in spreadsheetMaps)
        {
            Sheet? wSheet = wbPart?.Workbook?.Descendants<Sheet>().FirstOrDefault(s => s.Name == spreadsheetMap.Worksheet);
            if (wSheet is null || wSheet.Id is null)
            {
                errors.Add($"Sheet '{spreadsheetMap.Worksheet}' not found in the spreadsheet.");
                continue;
            }

            WorksheetPart? wsPart = wbPart.GetPartById(wSheet.Id!) as WorksheetPart;
            if (wsPart is null)
            {
                errors.Add($"WorksheetPart for sheet '{spreadsheetMap.Worksheet}' not found.");
                continue;
            }

            if (spreadsheetMap.DataMaps == null || !spreadsheetMap.DataMaps.Any()) continue;

            foreach (var dataMap in spreadsheetMap.DataMaps)
            {
                Cell? cell = wsPart.Worksheet?.Descendants<Cell>()?.FirstOrDefault(c => c.CellReference == dataMap.Cell);
                if (cell is null)
                {
                    errors.Add($"Cell '{dataMap.Cell}' not found in worksheet {spreadsheetMap.Worksheet}.");
                    continue;
                }

                string? cellValue;
                if (cell is null || cell.InnerText.Length < 0)
                {
                    data.Add(dataMap.Name!, null);
                    continue;
                }

                cellValue = cell.InnerText;
                if (cell.DataType is not null)
                {
                    if (cell.DataType.Value == CellValues.SharedString)
                    {
                        var stringTable = wbPart!.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                        if (stringTable is not null)
                        {
                            cellValue = stringTable.SharedStringTable!.ElementAt(int.Parse(cellValue)).InnerText;
                        }
                    }
                    else if (cell.DataType.Value == CellValues.Boolean)
                    {
                        cellValue = cellValue switch
                        {
                            "0" => "FALSE",
                            _ => "TRUE",
                        };
                    }
                }
                data.Add(dataMap.Name!, cellValue);
            }
        }

        return Task.FromResult((object)data);
    }
}
