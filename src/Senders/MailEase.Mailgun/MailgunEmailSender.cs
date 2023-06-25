using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using MailEase.Extensions;
using MailEase.Mailgun.Extensions;
using MailEase.Results;

namespace MailEase.Mailgun;

public class MailgunEmailSender : IEmailSender
{
    private readonly HttpClient _httpClient;

    public MailgunEmailSender(MailgunConfiguration mailgunConfiguration)
    {
        var url = mailgunConfiguration.Region switch
        {
            MailGunRegion.Eu => $"https://api.eu.mailgun.net/v3/{mailgunConfiguration.DomainName}/",
            MailGunRegion.Usa => $"https://api.mailgun.net/v3/{mailgunConfiguration.DomainName}/",
            _ => throw new ArgumentOutOfRangeException(nameof(mailgunConfiguration.Region), mailgunConfiguration.Region, null)
        };
        
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(url)
        };
        
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{mailgunConfiguration.ApiKey}")));
    }
    
    public MailgunEmailSender(Func<MailgunConfiguration> mailgunConfigurationFactory) : this(mailgunConfigurationFactory())
    {
    }

    public async Task<SendEmailResult> SendAsync(IMailEaseEmail email, CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string>();
        
        parameters.Add("from", email.Data.From.ToMailgunAddress());
        parameters.AddRange(email.Data.To.Select(to => new KeyValuePair<string, string>("to", to.ToMailgunAddress())));
        parameters.AddRange(email.Data.Cc.Select(cc => new KeyValuePair<string, string>("cc", cc.ToMailgunAddress())));
        parameters.AddRange(email.Data.Bcc.Select(bcc => new KeyValuePair<string, string>("bcc", bcc.ToMailgunAddress())));
        parameters.AddRange(email.Data.ReplyTo.Select(replyTo => new KeyValuePair<string, string>("h:Reply-To", replyTo.ToMailgunAddress())));
        
        parameters.Add("subject", email.Data.Subject);
        parameters.Add(email.Data.Body.IsHtml ? "html" : "text", email.Data.Body.Content);
        
        if (!string.IsNullOrWhiteSpace(email.Data.Body.PlainTextAlternativeBody))
            parameters.Add("text", email.Data.Body.PlainTextAlternativeBody);
        
        parameters.AddRange(email.Data.Tags.Select(tag => new KeyValuePair<string, string>("o:tag", tag)));
        parameters.AddRange(email.Data.Headers.Select(header => new KeyValuePair<string, string>(header.Key.StartsWith("h:") ? header.Key : $"h:{header.Key}", header.Value)));
        parameters.AddRange(email.Data.Variables.Select(variable => new KeyValuePair<string, string>($"v:{variable.Key}", variable.Value)));
        
        if (email.Data.IsSandboxMode)
            parameters.Add("o:testmode", "true");

        var multipartFormDataContent = new MultipartFormDataContent();
        parameters.ForEach(parameter => multipartFormDataContent.Add(new StringContent(parameter.Value), parameter.Key));

        foreach (var attachment in email.Data.Attachments)
        {
            multipartFormDataContent.Add(new ByteArrayContent(await attachment.ToByteArrayAsync(cancellationToken)), attachment.IsInline ? "inline" : "attachment", attachment.FileName);
        }
        
        var httpResponseMessage = await _httpClient.PostAsync("messages", multipartFormDataContent, cancellationToken);

        var response = await httpResponseMessage.Content.ReadFromJsonAsync<MailgunResponse>(cancellationToken: cancellationToken);
        
        var result = new SendEmailResult { MessageId = response?.Id ?? string.Empty };
        if (!httpResponseMessage.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(response?.Message))
        {
            result.Errors.Add(response.Message);
        }

        return result;
    }
}