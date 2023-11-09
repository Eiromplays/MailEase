using MailEase.Providers.Infobip;

namespace MailEase.Test.Providers;

public sealed class InfobipTests
{
    private readonly IEmailProvider<InfobipMessage> _emailProvider;

    public InfobipTests()
    {
        var apiKey =
            Environment.GetEnvironmentVariable("INFOBIP_API_KEY") ?? "YOUR_INFOBIP_API_KEY";
        var baseAddress = new Uri(
            Environment.GetEnvironmentVariable("INFOBIP_BASE_URL") ?? "YOUR_INFOBIP_BASE_URL"
        );

        _emailProvider = Emails.Infobip(new InfobipParams(apiKey, baseAddress));
    }

    public const string Subject = "MailEase";
    public const string From = "YOUR_FROM_EMAIL_HERE";
    public const string To = "YOUR_TO_EMAIL_HERE";

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
            Subject = Subject,
            From = From,
            ToAddresses = new List<EmailAddress> { new(To, "MailEase") },
            Body = "<h1>Hello</h1>"
        };

        await _emailProvider.SendEmailAsync(request);
    }

    [Fact]
    public async Task SendEmailWithInvalidSendAt()
    {
        var request = new InfobipMessage
        {
            Subject = Subject,
            From = From,
            ToAddresses = new List<EmailAddress> { new(To, "MailEase") },
            Body = "<h1>Hello</h1>",
            SendAt = DateTimeOffset.UtcNow.AddDays(30).AddSeconds(1)
        };

        var sendEmailAsync = async () => await _emailProvider.SendEmailAsync(request);

        await sendEmailAsync
            .Should()
            .ThrowAsync<MailEaseException>()
            .Where(x => x.Errors.Any(y => y.ErrorCode == MailEaseErrorCode.InvalidSendAt));
    }
}
