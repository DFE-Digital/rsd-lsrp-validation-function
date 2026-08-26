using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using GovUK.Dfe.Lsrp.FileValidator.Models;
using GovUK.Dfe.Lsrp.FileValidator.Services;

namespace GovUK.Dfe.Lsrp.FileValidator.Tests;

public class SpreadsheetDataProviderTests
{
    [Fact]
    public void GetData_WhenSheetMissing_AddsError()
    {
        SpreadsheetDataProvider provider = new();
        using MemoryStream stream = CreateSpreadsheetStream([new SheetDefinition("Sheet1", [new CellDefinition("A1", "Value")])]);
        List<string> errors = [];
        List<SpreadsheetMap> maps = [new SpreadsheetMap { Worksheet = "Missing", DataMaps = [new DataMap { Name = "Field1", Cell = "A1" }] }];

        var result = provider.GetData(stream, maps, errors);

        Assert.NotNull(result);
        Assert.Single(errors);
        Assert.Equal("Sheet 'Missing' not found in the spreadsheet.", errors[0]);
    }

    [Fact]
    public void GetData_WhenCellMissing_AddsError()
    {
        SpreadsheetDataProvider provider = new();
        using MemoryStream stream = CreateSpreadsheetStream([new SheetDefinition("Sheet1", [new CellDefinition("B1", "Value")])]);
        List<string> errors = [];
        List<SpreadsheetMap> maps = [new SpreadsheetMap { Worksheet = "Sheet1", DataMaps = [new DataMap { Name = "Field1", Cell = "A1" }] }];

        var result = provider.GetData(stream, maps, errors);

        Assert.NotNull(result);
        Assert.Single(errors);
        Assert.Equal("Cell 'A1' not found in worksheet Sheet1.", errors[0]);
    }

    [Fact]
    public void GetData_WhenCellIsSharedString_ResolvesValue()
    {
        SpreadsheetDataProvider provider = new();
        using MemoryStream stream = CreateSpreadsheetStream(
            [new SheetDefinition("Sheet1", [new CellDefinition("A1", "0", CellValues.SharedString)])],
            ["Resolved Value"]);
        List<string> errors = [];
        List<SpreadsheetMap> maps = [new SpreadsheetMap { Worksheet = "Sheet1", DataMaps = [new DataMap { Name = "Field1", Cell = "A1" }] }];

        var result = provider.GetData(stream, maps, errors);
        IDictionary<string, object?> data = Assert.IsAssignableFrom<IDictionary<string, object?>>(result);

        Assert.Empty(errors);
        Assert.Equal("Resolved Value", data["Field1"]);
    }

    [Fact]
    public void GetData_WhenCellIsBoolean_MapsToTrueFalseStrings()
    {
        SpreadsheetDataProvider provider = new();
        using MemoryStream stream = CreateSpreadsheetStream(
            [new SheetDefinition("Sheet1", [new CellDefinition("A1", "0", CellValues.Boolean), new CellDefinition("A2", "1", CellValues.Boolean)])]);
        List<string> errors = [];
        List<SpreadsheetMap> maps = [new SpreadsheetMap
        {
            Worksheet = "Sheet1",
            DataMaps = [new DataMap { Name = "FalseValue", Cell = "A1" }, new DataMap { Name = "TrueValue", Cell = "A2" }]
        }];

        var result = provider.GetData(stream, maps, errors);
        IDictionary<string, object?> data = Assert.IsAssignableFrom<IDictionary<string, object?>>(result);

        Assert.Empty(errors);
        Assert.Equal("FALSE", data["FalseValue"]);
        Assert.Equal("TRUE", data["TrueValue"]);
    }

    [Fact]
    public void GetData_WhenCellIsEmpty_ReturnsNullForField()
    {
        SpreadsheetDataProvider provider = new();
        using MemoryStream stream = CreateSpreadsheetStream([new SheetDefinition("Sheet1", [new CellDefinition("A1", null)])]);
        List<string> errors = [];
        List<SpreadsheetMap> maps = [new SpreadsheetMap { Worksheet = "Sheet1", DataMaps = [new DataMap { Name = "Field1", Cell = "A1" }] }];

        var result = provider.GetData(stream, maps, errors);
        IDictionary<string, object?> data = Assert.IsAssignableFrom<IDictionary<string, object?>>(result);

        Assert.Empty(errors);
        Assert.True(data.ContainsKey("Field1"));
        Assert.Null(data["Field1"]);
    }

    private static MemoryStream CreateSpreadsheetStream(IEnumerable<SheetDefinition> sheets, IEnumerable<string>? sharedStrings = null)
    {
        MemoryStream stream = new();
        using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true))
        {
            WorkbookPart workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();
            Sheets workbookSheets = workbookPart.Workbook.AppendChild(new Sheets());

            if (sharedStrings is not null)
            {
                SharedStringTablePart sharedStringPart = workbookPart.AddNewPart<SharedStringTablePart>();
                sharedStringPart.SharedStringTable = new SharedStringTable(sharedStrings.Select(s => new SharedStringItem(new Text(s))));
            }

            uint sheetIndex = 1;
            foreach (SheetDefinition sheetDefinition in sheets)
            {
                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                SheetData sheetData = new();

                foreach (CellDefinition cellDefinition in sheetDefinition.Cells)
                {
                    Cell cell = new() { CellReference = cellDefinition.Reference };
                    if (cellDefinition.Value is not null)
                    {
                        cell.CellValue = new CellValue(cellDefinition.Value);
                    }

                    if (cellDefinition.DataType is not null)
                    {
                        cell.DataType = new EnumValue<CellValues>(cellDefinition.DataType.Value);
                    }

                    Row row = new();
                    row.Append(cell);
                    sheetData.Append(row);
                }

                worksheetPart.Worksheet = new Worksheet(sheetData);
                string relationshipId = workbookPart.GetIdOfPart(worksheetPart);

                workbookSheets.Append(new Sheet
                {
                    Id = relationshipId,
                    SheetId = sheetIndex++,
                    Name = sheetDefinition.Name
                });
            }

            workbookPart.Workbook.Save();
        }

        stream.Position = 0;
        return stream;
    }

    private sealed record SheetDefinition(string Name, IEnumerable<CellDefinition> Cells);

    private sealed record CellDefinition(string Reference, string? Value, CellValues? DataType = null);
}