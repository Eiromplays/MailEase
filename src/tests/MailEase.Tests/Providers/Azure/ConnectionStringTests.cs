using MailEase.Providers.Microsoft;

namespace MailEase.Tests.Providers.Azure;

public class ConnectionStringTests
{
    [Fact]
    public void ParseConnectionString_ShouldSucceed()
    {
        const string connectionString =
            "endpoint=https://test.europe.communication.azure.com/;accesskey=accessKey";

        var parsedConnectionString = ConnectionString.Parse(connectionString);

        parsedConnectionString
            .GetRequired("endpoint")
            .Should()
            .Be("https://test.europe.communication.azure.com/");

        parsedConnectionString.GetRequired("accesskey").Should().Be("accessKey");
    }

    [Fact]
    public void ParseConnectionString_WithMissingEndpoint_ShouldThrow()
    {
        const string connectionString = "accesskey=accessKey";

        Action connectionStringFunc = () => ConnectionString.Parse(connectionString);

        connectionStringFunc
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage(
                "Azure Communication Email connection string must contain both an 'Endpoint' and a 'AccessKey'"
            );
    }

    [Fact]
    public void ParseConnectionString_WithMissingKey_ShouldThrow()
    {
        const string connectionString = "endpoint=https://test.europe.communication.azure.com/";

        Action connectionStringFunc = () => ConnectionString.Parse(connectionString);

        connectionStringFunc
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage(
                "Azure Communication Email connection string must contain both an 'Endpoint' and a 'AccessKey'"
            );
    }

    [Fact]
    public void ParseConnectionString_WithEmptyConnectionString_ShouldThrow()
    {
        const string connectionString = "";

        var connectionStringFunc = () => ConnectionString.Parse(connectionString);

        connectionStringFunc
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Connection string cannot be empty.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void ParseConnectionString_WithNullOrWhiteSpaceConnectionString_ShouldThrow(
        string connectionString
    )
    {
        Action connectionStringFunc = () => ConnectionString.Parse(connectionString);

        connectionStringFunc
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Connection string cannot be empty.");
    }

    [Fact]
    public void ParseConnectionString_WithMoreThanExpectedKeys_ShouldSucceed()
    {
        const string connectionString =
            "endpoint=https://test.europe.communication.azure.com/;accesskey=accessKey;additionalKey=additionalValue";

        var parsedConnectionString = ConnectionString.Parse(connectionString);

        parsedConnectionString
            .GetRequired("endpoint")
            .Should()
            .Be("https://test.europe.communication.azure.com/");

        parsedConnectionString.GetRequired("accesskey").Should().Be("accessKey");

        parsedConnectionString.GetRequired("additionalKey").Should().Be("additionalValue");
    }

    [Fact]
    public void ParseConnectionString_WithDuplicateKeys_ShouldThrow()
    {
        const string connectionString =
            "endpoint=https://test.europe.communication.azure.com/;endpoint=https://duplicate.endpoint/;accesskey=accessKey";

        Action connectionStringFunc = () => ConnectionString.Parse(connectionString);

        connectionStringFunc
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Duplicated keyword 'endpoint'");
    }
}
