using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace MailEase.Providers.Microsoft;

internal class SharedKeyAuthHandler : DelegatingHandler
{
    public const string DateHeaderName = "x-ms-date";
    public const string MsContentSha256HeaderName = "x-ms-content-sha256";
    public const string AuthorizationHeaderName = "Authorization";
    private readonly string _accessKey;

    public SharedKeyAuthHandler(string accessKey)
        : base(new HttpClientHandler()) => _accessKey = accessKey;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        await SignAsync(request, cancellationToken: cancellationToken);
        AddHeaders(request, await CreateContentHashAsync(request, cancellationToken));
        return await base.SendAsync(request, cancellationToken);
    }

    protected override HttpResponseMessage Send(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        return SendAsync(request, cancellationToken).GetAwaiter().GetResult();
    }

    protected async Task SignAsync(
        HttpRequestMessage request,
        DateTimeOffset? signDate = null,
        CancellationToken cancellationToken = default
    )
    {
        var contentHash = await CreateContentHashAsync(request, cancellationToken);

        var dateToUse = signDate ?? DateTimeOffset.UtcNow;

        var utcNowString = dateToUse.ToString("r", CultureInfo.InvariantCulture);
        string? authorization = null;

        if (request.RequestUri is not null)
            authorization = GetAuthorizationHeader(
                request.Method,
                request.RequestUri,
                contentHash,
                utcNowString
            );

        request.Headers.Add(MsContentSha256HeaderName, contentHash);
        request.Headers.Add(DateHeaderName, utcNowString);
        request.Headers.Add(AuthorizationHeaderName, authorization);
    }

    private static async ValueTask<string> CreateContentHashAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var alg = SHA256.Create();

        using (var memoryStream = new MemoryStream())
        await using (
            var contentHashStream = new CryptoStream(memoryStream, alg, CryptoStreamMode.Write)
        )
        {
            if (request.Content is not null)
                await new StreamContent(await request.Content.ReadAsStreamAsync(cancellationToken))
                    .WriteToAsync(contentHashStream, cancellationToken)
                    .ConfigureAwait(false);
        }

        return Convert.ToBase64String(alg.Hash!);
    }

    private void AddHeaders(HttpRequestMessage request, string contentHash)
    {
        var utcNowString = DateTimeOffset.UtcNow.ToString("r", CultureInfo.InvariantCulture);
        string? authorization = null;

        if (request.RequestUri is not null)
            authorization = GetAuthorizationHeader(
                request.Method,
                request.RequestUri,
                contentHash,
                utcNowString
            );

        request.Headers.Add(MsContentSha256HeaderName, contentHash);
        request.Headers.Add(DateHeaderName, utcNowString);
        request.Headers.Add(AuthorizationHeaderName, authorization);
    }

    private string GetAuthorizationHeader(
        HttpMethod method,
        Uri uri,
        string contentHash,
        string date
    )
    {
        var host = uri.Authority;
        var pathAndQuery = uri.PathAndQuery;

        var stringToSign = $"{method.Method}\n{pathAndQuery}\n{date};{host};{contentHash}";
        var signature = ComputeHmac(stringToSign);

        const string signedHeaders = $"{DateHeaderName};host;x-ms-content-sha256";
        return $"HMAC-SHA256 SignedHeaders={signedHeaders}&Signature={signature}";
    }

    private string ComputeHmac(string value)
    {
        using var hmac = new HMACSHA256(Convert.FromBase64String(_accessKey));
        var hash = hmac.ComputeHash(Encoding.ASCII.GetBytes(value));
        return Convert.ToBase64String(hash);
    }
}

internal sealed class StreamContent : IDisposable
{
    private const int CopyToBufferSize = 81920;

    private readonly Stream _stream;

    private readonly long _origin;

    public StreamContent(Stream stream)
    {
        if (!stream.CanSeek)
            throw new ArgumentException("stream must be seekable", nameof(stream));
        _origin = stream.Position;
        _stream = stream;
    }

    public Task WriteToAsync(Stream stream, CancellationToken cancellation)
    {
        _stream.Seek(_origin, SeekOrigin.Begin);
        return _stream.CopyToAsync(stream, CopyToBufferSize, cancellation);
    }

    public void Dispose()
    {
        _stream.Dispose();
    }
}
