/*
 This file contains code derived from Stowage (https://github.com/aloneguid/stowage/blob/3b83e2af3925def45763a6ca052ae3f54a65cd55/src/Stowage/Impl/Amazon/S3AuthHandler.cs),
 under the Apache 2.0 license. See the 'licenses' directory for full license details.
*/

using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using MailEase.Utils;

namespace MailEase.Providers.Amazon;

/// <summary>
/// Handles Amazon SES authentication.
/// </summary>
internal class SesAuthHandler : DelegatingHandler
{
    public const string DateHeaderName = "x-amz-date";
    public const string AwsAuthorizationSchemeName = "AWS4-HMAC-SHA256";
    public const string AwsContentSha256HeaderName = "x-amz-content-sha256";
    public const string AwsSecurityTokenHeaderName = "x-amz-security-token";
    public const string AuthorizationHeaderName = "Authorization";

    private readonly string _accessKeyId;
    private readonly string _secretAccessKey;
    private readonly string? _sessionToken;
    private readonly string _region;
    private readonly string _service;
    private static readonly string EmptySha256 = Array.Empty<byte>().SHA256().ToHexString()!;

    public SesAuthHandler(
        string accessKeyId,
        string secretAccessKey,
        string? sessionToken,
        string region,
        string service = "ses"
    )
        : base(new HttpClientHandler())
    {
        _accessKeyId = accessKeyId;
        _secretAccessKey = secretAccessKey;
        _sessionToken = sessionToken;
        _region = region;
        _service = service;
    }

    /// <summary>
    /// Signs and sends an HTTP request asynchronously.
    /// </summary>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        await SignAsync(request);

        return await base.SendAsync(request, cancellationToken);
    }

#if NET5_0_OR_GREATER
    protected override HttpResponseMessage Send(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        return SendAsync(request, cancellationToken).GetAwaiter().GetResult();
    }
#endif

    protected async Task SignAsync(HttpRequestMessage request, DateTimeOffset? signDate = null)
    {
        // a very helpful article on S3 auth: https://docs.aws.amazon.com/AmazonS3/latest/API/sig-v4-header-based-auth.html

        var dateToUse = signDate ?? DateTimeOffset.UtcNow;
        var nowDate = dateToUse.ToString("yyyyMMdd");
        var amzNowDate = GetAmzDate(dateToUse);

        request.Headers.Add(DateHeaderName, amzNowDate);

        if (!string.IsNullOrWhiteSpace(_sessionToken))
            request.Headers.Add(AwsSecurityTokenHeaderName, _sessionToken);

        // 1. Create a canonical request

        /*
         * <HTTPMethod>\n
         * <CanonicalURI>\n
         * <CanonicalQueryString>\n
         * <CanonicalHeaders>\n
         * <SignedHeaders>\n
         * <HashedPayload>
         */

        var payloadHash = await AddPayloadHashHeader(request);

        var canonicalRequest =
            request.Method
            + "\n"
            + GetCanonicalUri(request)
            + "\n"
            + // CanonicalURI
            GetCanonicalQueryString(request)
            + "\n"
            + GetCanonicalHeaders(request, out var signedHeaders)
            + "\n"
            + // ends up with two newlines which is expected
            signedHeaders
            + "\n"
            + payloadHash;

        // 2. Create a string to sign

        // step by step instructions: https://docs.aws.amazon.com/general/latest/gr/sigv4-create-string-to-sign.html

        /*
         * StringToSign =
         *    Algorithm + \n +
         *    RequestDateTime + \n +
         *    CredentialScope + \n +
         *    HashedCanonicalRequest
         */

        var stringToSign =
            $"{AwsAuthorizationSchemeName}\n{amzNowDate}\n{nowDate}/{_region}/{_service}/aws4_request\n{canonicalRequest.SHA256()}";

        // 3. Calculate Signature

        /*
         * DateKey              = HMAC-SHA256("AWS4"+"<SecretAccessKey>", "<YYYYMMDD>")
         * DateRegionKey        = HMAC-SHA256(<DateKey>, "<aws-region>")
         * DateRegionServiceKey = HMAC-SHA256(<DateRegionKey>, "<aws-service>")
         * SigningKey           = HMAC-SHA256(<DateRegionServiceKey>, "aws4_request")
         */

        var kSecret = Encoding.UTF8.GetBytes(("AWS4" + _secretAccessKey).ToCharArray());
        var kDate = HmacSha256(nowDate, kSecret);
        var kRegion = HmacSha256(_region, kDate);
        var kService = HmacSha256(_service, kRegion);
        var kSigning = HmacSha256("aws4_request", kService);

        // final signature
        var signatureRaw = HmacSha256(stringToSign, kSigning);
        var signature = signatureRaw.ToHexString()!;

        var auth =
            $"Credential={_accessKeyId}/{nowDate}/{_region}/{_service}/aws4_request,SignedHeaders={signedHeaders},Signature={signature}";
        request.Headers.Authorization = new AuthenticationHeaderValue(
            AwsAuthorizationSchemeName,
            auth
        );
    }

    private static string GetAmzDate(DateTimeOffset date) => date.ToString("yyyyMMddTHHmmssZ");

    private static string GetCanonicalUri(HttpRequestMessage request)
    {
        var path = request.RequestUri!.GetAbsolutePathUnencoded();
        return AWSSDKUtils.UrlEncode(path, true);
    }

    private static string GetCanonicalQueryString(HttpRequestMessage request)
    {
        // CanonicalQueryString specifies the URI-encoded query string parameters. You URI-encode name and values individually. You must also sort the parameters in the canonical query string alphabetically by key name. The sorting occurs after encoding.

        var values = HttpUtility.ParseQueryString(request.RequestUri!.Query);
        var sb = new StringBuilder();

        // a. Sort the parameter names by character code point in ascending order. Parameters with duplicate names should be sorted by value.For example, a parameter name that begins with the uppercase letter F precedes a parameter name that begins with a lowercase letter b.

        foreach (var key in values.AllKeys.OrderBy(k => k))
        {
            if (sb.Length > 0)
            {
                sb.Append('&');
            }

            // URI-encode each parameter name and value.
            // This is a special encoding specific to AWS, not the standard URI encoding.
            var value = AWSSDKUtils.UrlEncode(values[key]!, false);

            if (key is null)
            {
                sb.Append(value).Append('=');
            }
            else
            {
                sb.Append(AWSSDKUtils.UrlEncode(key, false)).Append('=').Append(value);
            }
        }

        return sb.ToString();
    }

    private static string GetCanonicalHeaders(HttpRequestMessage request, out string signedHeaders)
    {
        // List of request headers with their values.
        // Individual header name and value pairs are separated by the newline character ("\n").
        // Header names must be in lowercase. You must sort the header names alphabetically to construct the string.

        // Note that I add some headers manually, but preserve sorting order in the actual code.

        var headers =
            from kvp in request.Headers
            where kvp.Key.StartsWith("x-amz-", StringComparison.OrdinalIgnoreCase)
            orderby kvp.Key
            select new { Key = kvp.Key.ToLowerInvariant(), kvp.Value };

        var sb = new StringBuilder();
        var signedHeadersList = new List<string>();

        // The CanonicalHeaders list must include the following:
        // - HTTP host header.
        // - If the Content-Type header is present in the request, you must add it to the CanonicalHeaders list.
        // - Any x-amz-* headers that you plan to include in your request must also be added. For example, if you are using temporary security credentials, you need to include x-amz-security-token in your request. You must add this header in the list of CanonicalHeaders.

        var contentType = request.Content?.Headers.ContentType?.ToString();
        if (contentType is not null)
        {
            sb.Append("content-type:").Append(contentType).Append('\n');
            signedHeadersList.Add("content-type");
        }

        if (request.Headers.Contains("date"))
        {
            sb.Append("date:").Append(request.Headers.GetValues("date").First()).Append('\n');
            signedHeadersList.Add("date");
        }

        sb.Append("host:").Append(request.RequestUri?.Authority).Append('\n');
        signedHeadersList.Add("host");

        if (request.Headers.Contains("range"))
        {
            sb.Append("range:").Append(request.Headers.GetValues("range").First()).Append('\n');
            signedHeadersList.Add("range");
        }

        // Create the string in the right format; this is what makes the headers "canonicalized" --
        //   it means put in a standard format. http://en.wikipedia.org/wiki/Canonicalization
        foreach (var kvp in headers)
        {
            sb.Append(kvp.Key).Append(':');
            signedHeadersList.Add(kvp.Key);

            foreach (var hv in kvp.Value)
            {
                sb.Append(hv);
            }

            sb.Append('\n');
        }

        signedHeaders = string.Join(";", signedHeadersList);

        return sb.ToString();
    }

    /// <summary>
    /// Hex(SHA256Hash(&lt;payload&gt;))
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private static async Task<string> AddPayloadHashHeader(HttpRequestMessage request)
    {
        string hash;

        if (request.Content is not null)
        {
            var content = await request.Content.ReadAsByteArrayAsync();
            hash = content.SHA256().ToHexString()!;
        }
        else
        {
            hash = EmptySha256;
        }

        request.Headers.Add(AwsContentSha256HeaderName, hash);

        return hash;
    }

    private static byte[] HmacSha256(string data, byte[] key)
    {
        var alg = new HMACSHA256(key);
        if (alg is null)
        {
            throw new InvalidOperationException("HmacSHA256 could not be instantiated");
        }
        return alg.ComputeHash(Encoding.UTF8.GetBytes(data));
    }
}
