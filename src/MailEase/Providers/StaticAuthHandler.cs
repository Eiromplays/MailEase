using System.Net.Http.Headers;

namespace MailEase.Providers;

public abstract record AuthToken(string Value)
{
    public abstract AuthenticationHeaderValue GetAuthenticationHeader();
}

public sealed record AppToken(string Value) : AuthToken(Value)
{
    public override AuthenticationHeaderValue GetAuthenticationHeader()
    {
        if (string.IsNullOrWhiteSpace(Value))
            throw new ArgumentNullException(nameof(Value), "App token cannot be empty.");
        return new AuthenticationHeaderValue("App", Value);
    }
}

public sealed record BearerToken(string Value) : AuthToken(Value)
{
    public override AuthenticationHeaderValue GetAuthenticationHeader()
    {
        if (string.IsNullOrWhiteSpace(Value))
            throw new ArgumentNullException(nameof(Value), "Bearer token cannot be empty.");
        return new AuthenticationHeaderValue("Bearer", Value);
    }
}

public sealed class StaticAuthHandler : DelegatingHandler
{
    private readonly AuthenticationHeaderValue _authenticationHeaderValue;

    public StaticAuthHandler(AuthToken authToken)
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
