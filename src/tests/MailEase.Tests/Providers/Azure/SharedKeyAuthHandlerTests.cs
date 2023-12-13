using System.Globalization;
using System.Text;
using MailEase.Providers.Microsoft;

namespace MailEase.Tests.Providers.Azure;

internal sealed class SharedKeyAuthHandlerWrapper : SharedKeyAuthHandler
{
    public SharedKeyAuthHandlerWrapper(string accessKey)
        : base(accessKey) { }

    public async Task<HttpRequestMessage> ExecuteRequestAsync(
        HttpMethod method,
        DateTimeOffset date,
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

        await SignAsync(request, date);

        return request;
    }
}

public sealed class SharedKeyAuthHandlerTests
{
    private static void CheckHeader(
        HttpRequestMessage requestMessage,
        string headerName,
        string expectedHeaderValue
    )
    {
        requestMessage.Headers.Contains(headerName).Should().BeTrue();
        requestMessage.Headers.GetValues(headerName).First().Should().Be(expectedHeaderValue);
    }

    private static readonly SharedKeyAuthHandlerWrapper Handler =
        new(Convert.ToBase64String(Encoding.UTF8.GetBytes("accessKey")));

    // This is a placeholder test (assuming "X-My-Header" is a key in the header collection)
    // And the expected value for this key is "expectedValue"
    [Fact]
    public async Task ExecuteRequestAsync_ShouldInsertHeader_WhenExtraHeadersProvided()
    {
        // Arrange
        var extraHeaders = new Dictionary<string, string> { { "X-My-Header", "expectedValue" } };

        // Act
        var result = await Handler.ExecuteRequestAsync(
            HttpMethod.Get,
            DateTimeOffset.UtcNow,
            extraHeaders: extraHeaders
        );

        // Assert
        CheckHeader(result, "X-My-Header", "expectedValue");
    }

    // Assuming the "X-Amz-Date" header is added by the SignAsync method
    [Fact]
    public async Task ExecuteRequestAsync_ShouldInsertDateHeader()
    {
        // Arrange
        var date = DateTimeOffset.UtcNow;
        var expectedHeaderValue = date.ToString("r", CultureInfo.InvariantCulture);

        // Act
        var result = await Handler.ExecuteRequestAsync(HttpMethod.Get, date);

        // Assert
        CheckHeader(result, SharedKeyAuthHandler.DateHeaderName, expectedHeaderValue);
    }

    // Assuming the "Authorization" header is added by the SignAsync method
    // Note: You need to replace "expectedSignature" with the real expected authorization header value
    [Fact]
    public async Task ExecuteRequestAsync_ShouldInsertAuthorizationHeader()
    {
        // Arrange
        var date = new DateTime(2023, 12, 27, 0, 0, 0, DateTimeKind.Utc);
        const string expectedSignature =
            "HMAC-SHA256 SignedHeaders=x-ms-date;host;x-ms-content-sha256&Signature=Y/QjmS9N+yVnHqARODZ2wujyxSuPW2PLW7azhyRZpm8=";

        // Act
        var result = await Handler.ExecuteRequestAsync(HttpMethod.Get, date);

        // Assert
        CheckHeader(result, SharedKeyAuthHandler.AuthorizationHeaderName, expectedSignature);
    }

    [Fact]
    public async Task ExecuteRequestAsync_ShouldInsertContentSha256Header()
    {
        // Arrange
        var date = new DateTime(2023, 12, 27);

        // Act
        var result = await Handler.ExecuteRequestAsync(HttpMethod.Get, date);

        // Assert
        CheckHeader(
            result,
            SharedKeyAuthHandler.MsContentSha256HeaderName,
            "47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU="
        );
    }
}
