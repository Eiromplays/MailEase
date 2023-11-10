using MailEase.Providers.Infobip;

namespace MailEase.Test.Providers;

public sealed class InfobipTests
{
    private readonly IEmailProvider<InfobipMessage> _emailProvider;
    private readonly string _subject = "MailEase";
    private readonly string _from;
    private readonly string _to;

    public InfobipTests()
    {
        var apiKey =
            Environment.GetEnvironmentVariable("INFOBIP_API_KEY")
            ?? throw new InvalidOperationException("Infobip API key cannot be empty.");
        var baseAddress = new Uri(
            Environment.GetEnvironmentVariable("INFOBIP_BASE_URL")
                ?? throw new InvalidOperationException("Infobip base URL cannot be empty.")
        );

        _subject = Environment.GetEnvironmentVariable("INFOBIP_SUBJECT") ?? _subject;
        _from =
            Environment.GetEnvironmentVariable("INFOBIP_FROM")
            ?? throw new InvalidOperationException("FROM cannot be empty.");
        _to =
            Environment.GetEnvironmentVariable("INFOBIP_TO")
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
            Body = "<h1>Hello</h1>"
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