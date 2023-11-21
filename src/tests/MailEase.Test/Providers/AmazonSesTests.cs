using System.Text;
using MailEase.Providers.Amazon;
using Microsoft.Extensions.Configuration;

namespace MailEase.Test.Providers;

public sealed class AmazonSesTests
{
    private readonly IEmailProvider<AmazonSesMessage> _emailProvider;
    private readonly string _subject = "MailEase";
    private readonly string _from;
    private readonly string _to;

    public AmazonSesTests()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json", true)
            .AddEnvironmentVariables()
            .Build();

        var accessKeyId =
            config.GetValue<string>("AMAZON_SES_ACCESS_KEY_ID")
            ?? throw new InvalidOperationException("Access key ID cannot be empty.");

        var secretAccessKey =
            config.GetValue<string>("AMAZON_SES_SECRET_ACCESS_KEY")
            ?? throw new InvalidOperationException("Secret access key cannot be empty.");

        var region =
            config.GetValue<string>("AMAZON_SES_REGION")
            ?? throw new InvalidOperationException("Region cannot be empty.");

        _subject = config.GetValue<string>("AMAZON_SES_SUBJECT") ?? _subject;
        _from =
            config.GetValue<string>("AMAZON_SES_FROM")
            ?? throw new InvalidOperationException("FROM cannot be empty.");
        _to =
            config.GetValue<string>("AMAZON_SES_TO")
            ?? throw new InvalidOperationException("TO cannot be empty.");

        _emailProvider = Emails.AmazonSes(
            new AmazonSesParams(accessKeyId, secretAccessKey, region)
        );
    }

    [Fact]
    public async Task SendEmail()
    {
        var request = new AmazonSesMessage
        {
            Subject = _subject,
            From = _from,
            ToAddresses = new List<EmailAddress> { new(_to) },
            Body = "<h1>Hello</h1>",
            IsHtmlBody = true
        };

        await _emailProvider.SendEmailAsync(request);
    }

    [Fact]
    public async Task SendEmailWithAttachment()
    {
        var attachment = new EmailAttachment(
            "MyVerySecretAttachment.txt",
            new MemoryStream(Encoding.UTF8.GetBytes("Hello World!")),
            "text/plain"
        );

        var request = new AmazonSesMessage
        {
            Subject = _subject,
            From = _from,
            ToAddresses = new List<EmailAddress> { new(_to, "MailEase") },
            Attachments = new List<EmailAttachment> { attachment },
            Body = "<h1>Hello</h1>",
            IsHtmlBody = true
        };

        await _emailProvider.SendEmailAsync(request);
    }
}
