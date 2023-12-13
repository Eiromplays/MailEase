// Original source code: https://github.com/aloneguid/stowage/blob/master/src/Stowage/Impl/Microsoft/EntraIdAuthHandler.cs

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace MailEase.Providers.Microsoft;

public sealed record ClientSecretCredential(string TenantId, string ClientId, string ClientSecret)
{
    /// <summary>
    /// Tenant id. Usually looks like a GUID.
    /// </summary>
    public string TenantId { get; init; } = TenantId;

    /// <summary>
    /// Client id
    /// </summary>
    public string ClientId { get; init; } = ClientId;

    /// <summary>
    /// Client secret
    /// </summary>
    public string ClientSecret { get; init; } = ClientSecret;
}

internal class EntraIdAuthHandler : DelegatingHandler
{
    private const string Scope = "https://communication.azure.com//.default";
    public const string AzureServiceVersionHeaderName = "x-ms-version";
    public const string AzureServiceVersion = "2023-11-03";
    public const string AuthorizationHeaderName = "Authorization";

    private readonly HttpClient _authClient = new();
    private readonly ClientSecretCredential _clientSecretCredential;
    private TokenResponse? _token;

    public EntraIdAuthHandler(ClientSecretCredential clientSecretCredential)
        : base(new HttpClientHandler()) => _clientSecretCredential = clientSecretCredential;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        await AuthenticateAsync(request);

        return await base.SendAsync(request, cancellationToken);
    }

    protected override HttpResponseMessage Send(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    ) => SendAsync(request, cancellationToken).GetAwaiter().GetResult();

    private async Task ObtainTokenAsync()
    {
        var tokenRequestUrl =
            $"https://login.microsoftonline.com/{_clientSecretCredential.TenantId}/oauth2/v2.0/token";
        var nvp = new Dictionary<string, string>
        {
            ["client_id"] = _clientSecretCredential.ClientId,
            ["scope"] = Scope,
            ["client_secret"] = _clientSecretCredential.ClientSecret,
            ["grant_type"] = "client_credentials"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, tokenRequestUrl)
        {
            Content = new FormUrlEncodedContent(nvp)
        };

        var response = await _authClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        _token = await response.Content.ReadFromJsonAsync<TokenResponse>();
    }

    protected async Task AuthenticateAsync(HttpRequestMessage request)
    {
        request.Headers.Add(AzureServiceVersionHeaderName, AzureServiceVersion);

        if (_token is null)
            await ObtainTokenAsync();

        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _token!.AccessToken
        );
    }
}

internal sealed class TokenResponse
{
    /// <summary>
    /// The only type that Microsoft Entra ID supports is Bearer.
    /// </summary>
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int? ExpiresInSeconds { get; set; }

    /// <summary>
    /// Used to indicate an extended lifetime for the access token and to support resiliency when the token issuance service isn't responding.
    /// </summary>
    [JsonPropertyName("ext_expires_in")]
    public int? ExtExpiresInSeconds { get; set; }

    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
}
