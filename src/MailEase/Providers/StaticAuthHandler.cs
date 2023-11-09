using System.Net.Http.Headers;

namespace MailEase.Providers;

public interface IAuthToken
{
    string Value { get; }
    AuthenticationHeaderValue GetAuthenticationHeader();
}

public readonly struct AppToken : IAuthToken
{
    public string Value { get; }

    public AppToken(string value) => Value = value;

    public AuthenticationHeaderValue GetAuthenticationHeader()
    {
        if (string.IsNullOrWhiteSpace(Value))
            throw new ArgumentNullException(nameof(Value), "App token cannot be empty.");
        return new AuthenticationHeaderValue("App", Value);
    }
}

public readonly struct BearerToken : IAuthToken
{
    public string Value { get; }

    public BearerToken(string value) => Value = value;

    public AuthenticationHeaderValue GetAuthenticationHeader()
    {
        if (string.IsNullOrWhiteSpace(Value))
            throw new ArgumentNullException(nameof(Value), "Bearer token cannot be empty.");
        return new AuthenticationHeaderValue("Bearer", Value);
    }
}

public sealed class StaticAuthHandler : DelegatingHandler
{
    private readonly AuthenticationHeaderValue _authenticationHeaderValue;

    public StaticAuthHandler(IAuthToken authToken)
        : base(new HttpClientHandler())
    {
        _authenticationHeaderValue = authToken.GetAuthenticationHeader();
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        request.Headers.Authorization = _authenticationHeaderValue;
        return await base.SendAsync(request, cancellationToken);
    }

    protected override HttpResponseMessage Send(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        request.Headers.Authorization = _authenticationHeaderValue;
        return base.Send(request, cancellationToken);
    }
}
