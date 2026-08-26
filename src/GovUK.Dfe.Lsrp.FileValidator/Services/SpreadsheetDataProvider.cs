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
                if (string.IsNullOrEmpty(cell.InnerText))
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

            if (spreadsheetMap.DataColumns != null && spreadsheetMap.DataColumns.Any())
            {
                IEnumerable<Cell>? cells = wsPart.Worksheet?.Descendants<Cell>();
                if (cells != null && cells.Any())
                {
                    foreach (DataColumn dataColumn in spreadsheetMap.DataColumns)
                    {
                        if (string.IsNullOrEmpty(dataColumn.ColumnName)) continue;
                        var columnHasData = cells.Any(c => c.CellReference != null && c.CellReference.HasValue && c.CellReference.Value!.StartsWith(dataColumn.ColumnName) && !string.IsNullOrEmpty(c.InnerText));
                        if (dataColumn.HasData && !columnHasData)
                        {
                            errors.Add($"Column '{dataColumn.ColumnName}' is expected to have data but does not.");
                        }
                        else if (!dataColumn.HasData && columnHasData)
                        {
                            errors.Add($"Column '{dataColumn.ColumnName}' is not expected to have data but does.");
                        }
                    }
                }
            }
        }

        return Task.FromResult((object)data);
    }
}
