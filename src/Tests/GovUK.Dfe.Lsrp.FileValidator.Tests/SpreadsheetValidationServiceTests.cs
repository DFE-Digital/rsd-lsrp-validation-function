using GovUK.Dfe.Lsrp.FileValidator.Models;
using GovUK.Dfe.Lsrp.FileValidator.Services;
using Microsoft.Extensions.Options;
using NSubstitute;
using RulesEngine.Models;
using System.Text.Json;
using Xunit.Abstractions;

namespace GovUK.Dfe.Lsrp.FileValidator.Tests;

public class SpreadsheetValidationServiceTests(ITestOutputHelper output)
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

        User user = new() { LocalAuthority = "LA1" };

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ValidateAsync(user, "test.xlsx", errors));

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

        User user = new() { LocalAuthority = "LA1" };

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ValidateAsync(user, "test.xlsx", errors));

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

        User user = new() { LocalAuthority = "LA1" };

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ValidateAsync(user, "test.xlsx", errors));

        Assert.Equal("File stream null.", exception.Message);
    }

    [Fact]
    public async Task ValidateAsync_WhenSpreadsheetDataNull_ThrowsInvalidOperationException()
    {
        IFileProvider fileProvider = Substitute.For<IFileProvider>();
        fileProvider.GetFileAsync("test.xlsx").Returns(new MemoryStream());
        ISpreadsheetDataProvider dataProvider = Substitute.For<ISpreadsheetDataProvider>();
        dataProvider.GetData(Arg.Any<Stream>(), Arg.Any<IEnumerable<SpreadsheetMap>>(), Arg.Any<IList<string>>())
            .Returns((IDictionary<string, object?>)null!);
        IDataValidator dataValidator = Substitute.For<IDataValidator>();
        IOptions<ValidationOptions> options = CreateValidOptions();

        SpreadsheetValidationService service = new(fileProvider, dataProvider, dataValidator, options);
        List<string> errors = [];

        User user = new() { LocalAuthority = "LA1" };

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ValidateAsync(user, "test.xlsx", errors));

        Assert.Equal("Spreadsheet data null.", exception.Message);
    }

    [Fact]
    public async Task ValidateAsync_WhenNoErrors_CallsDataValidatorAndReturnsTrue()
    {
        IFileProvider fileProvider = Substitute.For<IFileProvider>();
        fileProvider.GetFileAsync("test.xlsx").Returns(new MemoryStream());
        ISpreadsheetDataProvider dataProvider = Substitute.For<ISpreadsheetDataProvider>();
        IDictionary<string, object?> spreadsheetData = new Dictionary<string, object?> { ["Value"] = "test" };
        dataProvider.GetData(Arg.Any<Stream>(), Arg.Any<IEnumerable<SpreadsheetMap>>(), Arg.Any<IList<string>>())
            .Returns(spreadsheetData);
        IDataValidator dataValidator = Substitute.For<IDataValidator>();
        dataValidator.ValidateAsync(Arg.Any<object>(), Arg.Any<User>(), Arg.Any<IEnumerable<Workflow>>(), Arg.Any<IList<string>>()).Returns(true);
        IOptions<ValidationOptions> options = CreateValidOptions();

        SpreadsheetValidationService service = new(fileProvider, dataProvider, dataValidator, options);
        List<string> errors = [];
        User user = new() { LocalAuthority = "LA1" };

        bool result = await service.ValidateAsync(user, "test.xlsx", errors);

        Assert.True(result);
        await dataValidator.Received(1).ValidateAsync(spreadsheetData, user, Arg.Any<IEnumerable<Workflow>>(), errors);
    }

    [Fact]
    public async Task ValidateAsync_WhenDataProviderAddsErrors_DoesNotCallDataValidatorAndReturnsFalse()
    {
        IFileProvider fileProvider = Substitute.For<IFileProvider>();
        fileProvider.GetFileAsync("test.xlsx").Returns(new MemoryStream());
        ISpreadsheetDataProvider dataProvider = Substitute.For<ISpreadsheetDataProvider>();
        IDictionary<string, object?> spreadsheetData = new Dictionary<string, object?> { ["Value"] = "test" };
        dataProvider.GetData(Arg.Any<Stream>(), Arg.Any<IEnumerable<SpreadsheetMap>>(), Arg.Any<IList<string>>())
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
        User user = new() { LocalAuthority = "LA1" };

        bool result = await service.ValidateAsync(user, "test.xlsx", errors);

        Assert.False(result);
        Assert.Single(errors);
        await dataValidator.DidNotReceive().ValidateAsync(Arg.Any<object>(), Arg.Any<User>(), Arg.Any<IEnumerable<Workflow>>(), Arg.Any<IList<string>>());
    }

    [Fact]
    public async Task ValidateAsync_WithValidData_ReturnsTrue()
    {
        const string filename = "qr-test.xlsx";
        IFileProvider fileProvider = Substitute.For<IFileProvider>();
        string excelPath = Path.Combine(AppContext.BaseDirectory, filename);
        fileProvider.GetFileAsync(filename).Returns(_ => new MemoryStream(File.ReadAllBytes(excelPath)));
        ISpreadsheetDataProvider dataProvider = new SpreadsheetDataProvider();

        IDataValidator dataValidator = new DataValidator();

        ValidationOptions validationOptions = JsonSerializer.Deserialize<ValidationOptions>(File.ReadAllText("validation-options.json")) ?? throw new InvalidOperationException("ValidationOptions missing in validation-options.json");
        IOptions<ValidationOptions> options = Substitute.For<IOptions<ValidationOptions>>();
        options.Value.Returns(validationOptions);

        SpreadsheetValidationService service = new(fileProvider, dataProvider, dataValidator, options);
        List<string> errors = [];
        User user = new() { LocalAuthority = "LA1" };
        bool result = await service.ValidateAsync(user, filename, errors);

        output.WriteLine($"Validation result: {result}. Errors: {string.Join(", ", errors)}");
        Assert.True(result);
        Assert.Empty(errors);
    }

    [Fact]
    public async Task ValidateAsync_WithInValidData_ReturnsFalse()
    {
        const string filename = "qr-test-x.xlsx";
        IFileProvider fileProvider = Substitute.For<IFileProvider>();
        string excelPath = Path.Combine(AppContext.BaseDirectory, filename);
        fileProvider.GetFileAsync(filename).Returns(_ => new MemoryStream(File.ReadAllBytes(excelPath)));
        ISpreadsheetDataProvider dataProvider = new SpreadsheetDataProvider();

        IDataValidator dataValidator = new DataValidator();

        ValidationOptions validationOptions = JsonSerializer.Deserialize<ValidationOptions>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "validation-options.json"))) ?? throw new InvalidOperationException("ValidationOptions missing in validation-options.json");
        IOptions<ValidationOptions> options = Substitute.For<IOptions<ValidationOptions>>();
        options.Value.Returns(validationOptions);

        SpreadsheetValidationService service = new(fileProvider, dataProvider, dataValidator, options);
        List<string> errors = [];
        User user = new() { LocalAuthority = "LA1" };
        bool result = await service.ValidateAsync(user, filename, errors);

        output.WriteLine($"Validation result: {result}. Errors: {string.Join(", ", errors)}");
        Assert.False(result);
        Assert.True(errors.Count > 0);
    }

    private static IOptions<ValidationOptions> CreateValidOptions()
        => Options.Create(new ValidationOptions
        {
            SpreadsheetMaps = [new SpreadsheetMap()],
            Workflows = [new Workflow()]
        });
}