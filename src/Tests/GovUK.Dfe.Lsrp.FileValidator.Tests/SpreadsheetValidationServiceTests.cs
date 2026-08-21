using GovUK.Dfe.Lsrp.FileValidator.Models;
using GovUK.Dfe.Lsrp.FileValidator.Services;
using Microsoft.Extensions.Options;
using NSubstitute;
using RulesEngine.Models;

namespace GovUK.Dfe.Lsrp.FileValidator.Tests;

public class SpreadsheetValidationServiceTests
{
    [Fact]
    public async Task ValidateAsync_WhenSpreadsheetMapsMissing_ThrowsInvalidOperationException()
    {
        IFileProvider fileProvider = Substitute.For<IFileProvider>();
        ISpreadsheetDataProvider dataProvider = Substitute.For<ISpreadsheetDataProvider>();
        IDataValidator dataValidator = Substitute.For<IDataValidator>();
        IOptions<ValidationOptions> options = Options.Create(new ValidationOptions
        {
            SpreadsheetMaps = null,
            Workflows = [new Workflow()]
        });

        SpreadsheetValidationService service = new(fileProvider, dataProvider, dataValidator, options);
        List<string> errors = [];

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ValidateAsync("test.xlsx", errors));

        Assert.Equal("Spreadsheet maps missing in configuration.", exception.Message);
    }

    [Fact]
    public async Task ValidateAsync_WhenWorkflowsMissing_ThrowsInvalidOperationException()
    {
        IFileProvider fileProvider = Substitute.For<IFileProvider>();
        ISpreadsheetDataProvider dataProvider = Substitute.For<ISpreadsheetDataProvider>();
        IDataValidator dataValidator = Substitute.For<IDataValidator>();
        IOptions<ValidationOptions> options = Options.Create(new ValidationOptions
        {
            SpreadsheetMaps = [new SpreadsheetMap()],
            Workflows = null
        });

        SpreadsheetValidationService service = new(fileProvider, dataProvider, dataValidator, options);
        List<string> errors = [];

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ValidateAsync("test.xlsx", errors));

        Assert.Equal("Workflows missing in configuration.", exception.Message);
    }

    [Fact]
    public async Task ValidateAsync_WhenFileStreamNull_ThrowsInvalidOperationException()
    {
        IFileProvider fileProvider = Substitute.For<IFileProvider>();
        fileProvider.GetFileAsync("test.xlsx").Returns(Task.FromResult<Stream>(null!));
        ISpreadsheetDataProvider dataProvider = Substitute.For<ISpreadsheetDataProvider>();
        IDataValidator dataValidator = Substitute.For<IDataValidator>();
        IOptions<ValidationOptions> options = CreateValidOptions();

        SpreadsheetValidationService service = new(fileProvider, dataProvider, dataValidator, options);
        List<string> errors = [];

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ValidateAsync("test.xlsx", errors));

        Assert.Equal("File stream null.", exception.Message);
    }

    [Fact]
    public async Task ValidateAsync_WhenSpreadsheetDataNull_ThrowsInvalidOperationException()
    {
        IFileProvider fileProvider = Substitute.For<IFileProvider>();
        fileProvider.GetFileAsync("test.xlsx").Returns(new MemoryStream());
        ISpreadsheetDataProvider dataProvider = Substitute.For<ISpreadsheetDataProvider>();
        dataProvider.GetDataAsync(Arg.Any<Stream>(), Arg.Any<IEnumerable<SpreadsheetMap>>(), Arg.Any<IList<string>>())
            .Returns(Task.FromResult<object>(null!));
        IDataValidator dataValidator = Substitute.For<IDataValidator>();
        IOptions<ValidationOptions> options = CreateValidOptions();

        SpreadsheetValidationService service = new(fileProvider, dataProvider, dataValidator, options);
        List<string> errors = [];

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ValidateAsync("test.xlsx", errors));

        Assert.Equal("Spreadsheet data null.", exception.Message);
    }

    [Fact]
    public async Task ValidateAsync_WhenNoErrors_CallsDataValidatorAndReturnsTrue()
    {
        IFileProvider fileProvider = Substitute.For<IFileProvider>();
        fileProvider.GetFileAsync("test.xlsx").Returns(new MemoryStream());
        ISpreadsheetDataProvider dataProvider = Substitute.For<ISpreadsheetDataProvider>();
        object spreadsheetData = new { Value = "test" };
        dataProvider.GetDataAsync(Arg.Any<Stream>(), Arg.Any<IEnumerable<SpreadsheetMap>>(), Arg.Any<IList<string>>())
            .Returns(spreadsheetData);
        IDataValidator dataValidator = Substitute.For<IDataValidator>();
        dataValidator.ValidateAsync(Arg.Any<object>(), Arg.Any<IEnumerable<Workflow>>(), Arg.Any<IList<string>>()).Returns(true);
        IOptions<ValidationOptions> options = CreateValidOptions();

        SpreadsheetValidationService service = new(fileProvider, dataProvider, dataValidator, options);
        List<string> errors = [];

        bool result = await service.ValidateAsync("test.xlsx", errors);

        Assert.True(result);
        await dataValidator.Received(1).ValidateAsync(spreadsheetData, Arg.Any<IEnumerable<Workflow>>(), errors);
    }

    [Fact]
    public async Task ValidateAsync_WhenDataProviderAddsErrors_DoesNotCallDataValidatorAndReturnsFalse()
    {
        IFileProvider fileProvider = Substitute.For<IFileProvider>();
        fileProvider.GetFileAsync("test.xlsx").Returns(new MemoryStream());
        ISpreadsheetDataProvider dataProvider = Substitute.For<ISpreadsheetDataProvider>();
        object spreadsheetData = new { Value = "test" };
        dataProvider.GetDataAsync(Arg.Any<Stream>(), Arg.Any<IEnumerable<SpreadsheetMap>>(), Arg.Any<IList<string>>())
            .Returns(call =>
            {
                IList<string> errorsArg = call.ArgAt<IList<string>>(2);
                errorsArg.Add("error");
                return spreadsheetData;
            });
        IDataValidator dataValidator = Substitute.For<IDataValidator>();
        IOptions<ValidationOptions> options = CreateValidOptions();

        SpreadsheetValidationService service = new(fileProvider, dataProvider, dataValidator, options);
        List<string> errors = [];

        bool result = await service.ValidateAsync("test.xlsx", errors);

        Assert.False(result);
        Assert.Single(errors);
        await dataValidator.DidNotReceive().ValidateAsync(Arg.Any<object>(), Arg.Any<IEnumerable<Workflow>>(), Arg.Any<IList<string>>());
    }

    private static IOptions<ValidationOptions> CreateValidOptions()
        => Options.Create(new ValidationOptions
        {
            SpreadsheetMaps = [new SpreadsheetMap()],
            Workflows = [new Workflow()]
        });
}