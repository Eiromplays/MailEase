using Microsoft.Extensions.Configuration;

namespace MailEase.Tests;

public class ConfigurationFixture
{
    public IConfigurationRoot Config { get; private set; }

    public ConfigurationFixture()
    {
        Config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json", true)
            .AddEnvironmentVariables()
            .Build();
    }
}
