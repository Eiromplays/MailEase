using Microsoft.Extensions.Configuration;

namespace MailEase.Tests;

public class ConfigurationFixture
{
    public IConfigurationRoot Config { get; private set; }

    public ConfigurationFixture()
    {
        Config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<ConfigurationFixture>()
            .Build();
    }
}
