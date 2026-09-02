using Microsoft.Extensions.Configuration;

namespace GovUK.Dfe.Lsrp.FileValidator.Tests;

public class TestConfig
{
    public static IConfiguration GetConfiguration()
    {
        return new ConfigurationBuilder()
            .AddJsonFile("testsettings.json", optional: true)
            .AddUserSecrets<TestConfig>(optional: true)
            .Build();
    }
}
