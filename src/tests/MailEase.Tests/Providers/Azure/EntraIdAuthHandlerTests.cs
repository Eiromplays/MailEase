using MailEase.Providers.Microsoft;
using Microsoft.Extensions.Configuration;

namespace MailEase.Tests.Providers.Azure;

internal sealed class EntraIdAuthHandlerWrapper : EntraIdAuthHandler
{
    public EntraIdAuthHandlerWrapper(string tenantId, string clientId, string clientSecret)
        : base(new ClientSecretCredential(tenantId, clientId, clientSecret)) { }

    public async Task<HttpRequestMessage> ExecuteRequestAsync(
        HttpMethod method,
        string url = "https://test.com",
        Dictionary<string, string>? extraHeaders = null,
        byte[]? body = null
    )
    {
        var request = new HttpRequestMessage(method, url);

        if (body is not null)
        {
            request.Content = new ByteArrayContent(body);
        }

        if (extraHeaders is not null)
        {
            foreach (var entry in extraHeaders)
            {
                request.Headers.Add(entry.Key, entry.Value);
            }
        }

        await AuthenticateAsync(request);

        return request;
    }
}

public sealed class EntraIdAuthHandlerTests : IClassFixture<ConfigurationFixture>
{
    private readonly EntraIdAuthHandlerWrapper _handler;

    public EntraIdAuthHandlerTests(ConfigurationFixture fixture)
    {
        var config = fixture.Config;

        var tenantId =
            config.GetValue<string>("Azure_TENANT_ID")
            ?? throw new InvalidOperationException("Azure tenant ID cannot be empty.");
        var clientId =
            config.GetValue<string>("Azure_CLIENT_ID")
            ?? throw new InvalidOperationException("Azure client ID cannot be empty.");
        var clientSecret =
            config.GetValue<string>("Azure_CLIENT_SECRET")
            ?? throw new InvalidOperationException("Azure client secret cannot be empty.");

        _handler = new EntraIdAuthHandlerWrapper(tenantId, clientId, clientSecret);
    }

    private static void CheckHeader(
        HttpRequestMessage requestMessage,
        string headerName,
        string expectedHeaderValue
    )
    {
        requestMessage.Headers.Contains(headerName).Should().BeTrue();
        requestMessage.Headers.GetValues(headerName).First().Should().Be(expectedHeaderValue);
    }

    // This is a placeholder test (assuming "X-My-Header" is a key in the header collection)
    // And the expected value for this key is "expectedValue"
    [Fact]
    public async Task ExecuteRequestAsync_ShouldInsertHeader_WhenExtraHeadersProvided()
    {
        // Arrange
        var extraHeaders = new Dictionary<string, string> { { "X-My-Header", "expectedValue" } };

        // Act
        var result = await _handler.ExecuteRequestAsync(HttpMethod.Get, extraHeaders: extraHeaders);

        // Assert
        CheckHeader(result, "X-My-Header", "expectedValue");
    }

    // Assuming the "Authorization" header is added by the SignAsync method
    // Note: You need to replace "ex" with the real expected authorization header value
    [Fact]
    public async Task ExecuteRequestAsync_ShouldInsertAuthorizationHeader()
    {
        // Arrange

        // Act
        var result = await _handler.ExecuteRequestAsync(HttpMethod.Get);

        // Assert
        result.Headers.Contains(EntraIdAuthHandler.AuthorizationHeaderName).Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteRequestAsync_ShouldInsertVersionHeader()
    {
        // Act
        var result = await _handler.ExecuteRequestAsync(HttpMethod.Get);

        // Assert
        CheckHeader(
            result,
            EntraIdAuthHandler.AzureServiceVersionHeaderName,
            EntraIdAuthHandler.AzureServiceVersion
        );
    }
}
