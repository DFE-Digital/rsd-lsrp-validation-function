using GovUK.Dfe.Lsrp.FileValidator.Services;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace GovUK.Dfe.Lsrp.FileValidator.Tests;

public class FakeLogger(ITestOutputHelper output) : ILogger<FileValidationResultService>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        output.WriteLine(formatter(state, exception));
    }
}
