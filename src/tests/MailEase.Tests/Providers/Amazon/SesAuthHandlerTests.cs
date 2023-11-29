using MailEase.Providers.Amazon;

namespace MailEase.Tests.Providers.Amazon;

internal sealed class SesAuthHandlerWrapper : SesAuthHandler
{
    public SesAuthHandlerWrapper(
        string accessKeyId,
        string secretAccessKey,
        string? sessionToken,
        string region,
        string service = "ses"
    )
        : base(accessKeyId, secretAccessKey, sessionToken, region, service) { }

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

public sealed class SesAuthHandlerTests
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

    private static readonly SesAuthHandlerWrapper Handler =
        new("accessKeyId", "secretAccessKey", "sessionToken", "region");

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
        var expectedHeaderValue = date.ToString("yyyyMMddTHHmmssZ");

        // Act
        var result = await Handler.ExecuteRequestAsync(HttpMethod.Get, date);

        // Assert
        CheckHeader(result, "X-Amz-Date", expectedHeaderValue);
    }

    // Assuming the "Authorization" header is added by the SignAsync method
    // Note: You need to replace "expectedSignature" with the real expected authorization header value
    [Fact]
    public async Task ExecuteRequestAsync_ShouldInsertAuthorizationHeader()
    {
        // Arrange
        var date = new DateTime(2023, 12, 27);
        const string expectedSignature =
            "AWS4-HMAC-SHA256 Credential=accessKeyId/20231227/region/ses/aws4_request,SignedHeaders=host;x-amz-content-sha256;x-amz-date;x-amz-security-token,Signature=f7b8c9079753551130efe7cbf49b0db4f33d3da0de663f2f600691a6b05d0569";

        // Act
        var result = await Handler.ExecuteRequestAsync(HttpMethod.Get, date);

        // Assert
        CheckHeader(result, "Authorization", expectedSignature);
    }
}
