using System.Buffers;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace MailEase.Providers.Microsoft;

internal class SharedKeyAuthHandler : DelegatingHandler
{
    private const string DateHeaderName = "x-ms-date";
    private const string AuthorizationHeaderName = "Authorization";
    private readonly string _accessKey;

    public SharedKeyAuthHandler(string accessKey)
        : base(new HttpClientHandler()) => _accessKey = accessKey;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        AddHeaders(request, await CreateContentHashAsync(request, cancellationToken));
        return await base.SendAsync(request, cancellationToken);
    }

    protected override HttpResponseMessage Send(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        AddHeaders(request, CreateContentHash(request, cancellationToken));
        return base.Send(request, cancellationToken);
    }

    private static string CreateContentHash(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var alg = SHA256.Create();

        using (var memoryStream = new MemoryStream())
        using (var contentHashStream = new CryptoStream(memoryStream, alg, CryptoStreamMode.Write))
        {
            if (request.Content is not null)
                new StreamContent(request.Content.ReadAsStream(cancellationToken)).WriteTo(
                    contentHashStream,
                    cancellationToken
                );
        }

        return Convert.ToBase64String(alg.Hash!);
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

        request.Headers.Add("x-ms-content-sha256", contentHash);
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

    public void WriteTo(Stream stream, CancellationToken cancellationToken)
    {
        _stream.Seek(_origin, SeekOrigin.Begin);

        // this is not using CopyTo so that we can honor cancellations.
        var buffer = ArrayPool<byte>.Shared.Rent(CopyToBufferSize);
        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var read = _stream.Read(buffer, 0, buffer.Length);
                if (read == 0)
                {
                    break;
                }
                cancellationToken.ThrowIfCancellationRequested();
                stream.Write(buffer, 0, read);
            }
        }
        finally
        {
            stream.Flush();
            ArrayPool<byte>.Shared.Return(buffer, true);
        }
    }

    public bool TryComputeLength(out long length)
    {
        if (_stream.CanSeek)
        {
            length = _stream.Length - _origin;
            return true;
        }
        length = 0;
        return false;
    }

    public async Task WriteToAsync(Stream stream, CancellationToken cancellation)
    {
        _stream.Seek(_origin, SeekOrigin.Begin);
        await _stream.CopyToAsync(stream, CopyToBufferSize, cancellation).ConfigureAwait(false);
    }

    public void Dispose()
    {
        _stream.Dispose();
    }
}
