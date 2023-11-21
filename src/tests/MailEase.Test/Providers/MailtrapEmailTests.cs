using System.Text;
using MailEase.Providers.Mailtrap;
using Microsoft.Extensions.Configuration;

namespace MailEase.Test.Providers;

public sealed class MailtrapEmailTests
{
    private readonly IEmailProvider<MailtrapMessage> _emailProvider;
    private readonly string _subject = "MailEase";
    private readonly string _from;
    private readonly string _to;

    public MailtrapEmailTests()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json", true)
            .AddEnvironmentVariables()
            .Build();

        var apiKey =
            config.GetValue<string>("MAILTRAP_API_KEY")
            ?? throw new InvalidOperationException("Mailtrap API key cannot be empty.");

        _subject = config.GetValue<string>("MAILTRAP_SUBJECT") ?? _subject;
        _from =
            config.GetValue<string>("MAILTRAP_FROM")
            ?? throw new InvalidOperationException("FROM cannot be empty.");
        _to =
            config.GetValue<string>("MAILTRAP_TO")
            ?? throw new InvalidOperationException("TO cannot be empty.");

        _emailProvider = Emails.Mailtrap(new MailtrapParams(apiKey));
    }

    [Fact]
    public void SendEmailWithEmptyApiKeyShouldThrow()
    {
        var mailtrapParamsFunc = () => Emails.Mailtrap(new MailtrapParams(""));
        mailtrapParamsFunc.Should().Throw<ArgumentNullException>("Bearer token cannot be empty.");
    }

    [Fact]
    public async Task SendEmail()
    {
        var request = new MailtrapMessage
        {
            Subject = _subject,
            From = new EmailAddress(_from, "MailEase"),
            ToAddresses = new List<EmailAddress> { new(_to, "MailEase") },
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

        var request = new MailtrapMessage
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
