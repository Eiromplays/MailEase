using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using MailEase.Exceptions;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Retry;

[assembly: InternalsVisibleTo("MailEase.Tests")]

namespace MailEase;

internal static class HttpPolicyOptions
{
    private static readonly IEnumerable<TimeSpan> Delay = Backoff.DecorrelatedJitterBackoffV2(
        medianFirstRetryDelay: TimeSpan.FromSeconds(1),
        retryCount: 5
    );
    public static readonly AsyncRetryPolicy<HttpResponseMessage> AsyncRetryPolicy =
        HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(Delay);
}

public abstract class BaseEmailProvider<TEmailMessage> : IEmailProvider<TEmailMessage>
    where TEmailMessage : IEmailMessage
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Default constructor for BaseEmailProvider.
    /// This constructor initializes an HttpClient with a default base address and is typically used for testing purposes and by the SMTP/MailKit provider which does not require a specific base address or authentication.
    /// </summary>
    internal BaseEmailProvider()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost"),
            DefaultRequestHeaders = { { "User-Agent", Constants.UserAgent }, }
        };
    }

    /// <summary>
    /// An overload of the BaseEmailProvider constructor which accepts specific base address and HttpMessageHandler for authentication.
    /// This constructor is used by email providers that require custom base address and specific authentication mechanisms.
    /// </summary>
    protected BaseEmailProvider(Uri baseAddress, HttpMessageHandler authenticationHandler)
    {
        ArgumentNullException.ThrowIfNull(baseAddress);

        _httpClient = new HttpClient(authenticationHandler)
        {
            BaseAddress = baseAddress,
            DefaultRequestHeaders = { { "User-Agent", Constants.UserAgent }, },
        };
    }

    public abstract Task SendEmailAsync(
        TEmailMessage message,
        CancellationToken cancellationToken = default
    );

    protected virtual void ValidateEmailMessage(TEmailMessage request)
    {
        var mailEaseException = new MailEaseException();

        if (!request.From.IsValid)
            mailEaseException.AddError(BaseEmailMessageErrors.InvalidFromAddress);

        if (string.IsNullOrWhiteSpace(request.Subject))
            mailEaseException.AddError(BaseEmailMessageErrors.InvalidSubject);

        if (request.ToAddresses.Count <= 0)
            mailEaseException.AddError(BaseEmailMessageErrors.NoRecipients);

        if (!request.ToAddresses.All(x => x.IsValid))
            mailEaseException.AddError(BaseEmailMessageErrors.InvalidToRecipients);

        if (!request.CcAddresses.All(x => x.IsValid))
            mailEaseException.AddError(BaseEmailMessageErrors.InvalidCcRecipients);

        if (!request.BccAddresses.All(x => x.IsValid))
            mailEaseException.AddError(BaseEmailMessageErrors.InvalidBccRecipients);

        if (!request.ReplyToAddresses.All(x => x.IsValid))
            mailEaseException.AddError(BaseEmailMessageErrors.InvalidReplyToRecipients);

        if (string.IsNullOrWhiteSpace(request.Html) && string.IsNullOrWhiteSpace(request.Text))
            mailEaseException.AddError(
                BaseEmailMessageErrors.InvalidBody("Both Html and Text cannot be empty.")
            );

        mailEaseException.AddErrors(ProviderSpecificValidation(request).Errors);

        if (mailEaseException.Errors.Count > 0)
            throw mailEaseException;
    }

    protected virtual MailEaseException ProviderSpecificValidation(TEmailMessage request)
    {
        return new MailEaseException();
    }

    protected Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default
    ) =>
        HttpPolicyOptions.AsyncRetryPolicy.ExecuteAsync(
            async () => await _httpClient.SendAsync(request, cancellationToken)
        );

    protected async Task<T> PostAsync<T>(
        string url,
        object? content = null,
        string? stringBody = null,
        string contentType = "application/json",
        bool throwOnError = true
    )
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);

        if (content is not null)
            request.Content = new StringContent(
                JsonSerializer.Serialize(content, content.GetType()),
                Encoding.UTF8,
                contentType
            );

        if (!string.IsNullOrWhiteSpace(stringBody))
            request.Content = new StringContent(stringBody, null, contentType);

        var response = await SendAsync(request);

        if (throwOnError)
            response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>()
            ?? throw new InvalidOperationException("Could not deserialize response.");
    }

    protected async Task PostAsync(
        string url,
        object? content = null,
        string? stringBody = null,
        string contentType = "application/json",
        bool throwOnError = true
    )
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);

        if (content is not null)
            request.Content = new StringContent(
                JsonSerializer.Serialize(content, content.GetType()),
                Encoding.UTF8,
                contentType
            );

        if (!string.IsNullOrWhiteSpace(stringBody))
            request.Content = new StringContent(stringBody, null, contentType);

        var response = await SendAsync(request);

        if (throwOnError)
            response.EnsureSuccessStatusCode();
    }

    public virtual Task<(TData?, TErrorResponse?)> PostJsonAsync<TData, TErrorResponse>(
        object content,
        bool throwOnError = true
    )
        where TData : class
        where TErrorResponse : class =>
        PostJsonAsync<TData, TErrorResponse>("", content, throwOnError: throwOnError);

    public virtual async Task<(TData?, TErrorResponse?)> PostJsonAsync<TData, TErrorResponse>(
        string url,
        object content,
        bool throwOnError = true
    )
        where TData : class
        where TErrorResponse : class
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(
            JsonSerializer.Serialize(content, content.GetType()),
            Encoding.UTF8,
            "application/json"
        );

        var response = await SendAsync(request);

        /*if (throwOnError)
            response.EnsureSuccessStatusCode();*/

        if (response.IsSuccessStatusCode)
            return (
                (
                    response.StatusCode != HttpStatusCode.NoContent
                    && response.Content.Headers.ContentLength > 0
                )
                    ? await response.Content.ReadFromJsonAsync<TData>()
                    : null,
                null
            );

        return (null, await response.Content.ReadFromJsonAsync<TErrorResponse>());
    }

    protected Task<(TData?, TErrorResponse?)> PostMultiPartFormDataAsync<TData, TErrorResponse>(
        MultipartFormDataContent content,
        bool throwOnError = true
    )
        where TData : class
        where TErrorResponse : class =>
        PostMultiPartFormDataAsync<TData, TErrorResponse>("", content, throwOnError: throwOnError);

    protected async Task<(TData?, TErrorResponse?)> PostMultiPartFormDataAsync<
        TData,
        TErrorResponse
    >(string url, MultipartFormDataContent content, bool throwOnError = true)
        where TData : class
        where TErrorResponse : class
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = content;

        var response = await SendAsync(request);

        /*if (throwOnError)
            response.EnsureSuccessStatusCode();*/

        if (response.IsSuccessStatusCode)
            return (
                (
                    response.StatusCode != HttpStatusCode.NoContent
                    && response.Content.Headers.ContentLength > 0
                )
                    ? await response.Content.ReadFromJsonAsync<TData>()
                    : null,
                null
            );

        return (null, await response.Content.ReadFromJsonAsync<TErrorResponse>());
    }

    protected async Task<T> GetAsync<T>(HttpRequestMessage request, bool throwOnError = true)
    {
        var response = await SendAsync(request);

        if (throwOnError)
            response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>()
            ?? throw new InvalidOperationException("Could not deserialize response.");
    }

    protected async Task<T> GetAsync<T>(string url, bool throwOnError = true) =>
        await GetAsync<T>(new HttpRequestMessage(HttpMethod.Get, url), throwOnError);

    public void Dispose()
    {
        _httpClient.Dispose();

        GC.SuppressFinalize(this);
    }
}
