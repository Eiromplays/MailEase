using System.Text;
using MailEase.Providers.Infobip;
using Microsoft.Extensions.Configuration;

namespace MailEase.Test.Providers;

public sealed class InfobipTests
{
    private readonly IEmailProvider<InfobipMessage> _emailProvider;
    private readonly string _subject = "MailEase";
    private readonly string _from;
    private readonly string _to;

    public InfobipTests()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json", true)
            .AddEnvironmentVariables()
            .Build();

        var apiKey =
            config.GetValue<string>("INFOBIP_API_KEY")
            ?? throw new InvalidOperationException("Infobip API key cannot be empty.");
        var baseAddress = new Uri(
            config.GetValue<string>("INFOBIP_BASE_URL")
                ?? throw new InvalidOperationException("Infobip base URL cannot be empty.")
        );

        _subject = config.GetValue<string>("INFOBIP_SUBJECT") ?? _subject;
        _from =
            config.GetValue<string>("INFOBIP_FROM")
            ?? throw new InvalidOperationException("FROM cannot be empty.");
        _to =
            config.GetValue<string>("INFOBIP_TO")
            ?? throw new InvalidOperationException("TO cannot be empty.");

        _emailProvider = Emails.Infobip(new InfobipParams(apiKey, baseAddress));
    }

    [Fact]
    public void SendEmailWithEmptyApiKeyShouldThrow()
    {
        var sendGridParamsFunc = () =>
            Emails.Infobip(new InfobipParams("", new Uri("https://api.infobip.com/")));
        sendGridParamsFunc.Should().Throw<ArgumentNullException>("App token cannot be empty.");
    }

    [Fact]
    public async Task SendEmail()
    {
        var request = new InfobipMessage
        {
            Subject = _subject,
            From = _from,
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

        var request = new InfobipMessage
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

    [Fact]
    public async Task SendEmailWithInvalidSendAt()
    {
        var request = new InfobipMessage
        {
            Subject = _subject,
            From = _from,
            ToAddresses = new List<EmailAddress> { new(_to, "MailEase") },
            Body = "<h1>Hello</h1>",
            SendAt = DateTimeOffset.UtcNow.AddDays(30).AddSeconds(1)
        };

        var sendEmailAsync = async () => await _emailProvider.SendEmailAsync(request);

        await sendEmailAsync
            .Should()
            .ThrowAsync<MailEaseException>()
            .Where(x => x.Errors.Any(y => y.Code == MailEaseErrorCode.InvalidSendAt));
    }
}
