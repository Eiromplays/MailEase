using MailEase.Providers.SendGrid;
using Xunit.Abstractions;

namespace MailEase.Test.Providers;

public sealed class SendGridTests
{
    private readonly IEmailProvider<SendGridMessage> _emailProvider;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly string _subject = "MailEase";
    private readonly string _from;
    private readonly string _to;

    public SendGridTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        var apiKey =
            Environment.GetEnvironmentVariable("SENDGRID_API_KEY")
            ?? throw new InvalidOperationException("SendGrid API key cannot be empty.");

        _subject = Environment.GetEnvironmentVariable("SENDGRID_SUBJECT") ?? _subject;
        _from =
            Environment.GetEnvironmentVariable("SENDGRID_FROM")
            ?? throw new InvalidOperationException("FROM cannot be empty.");
        _to =
            Environment.GetEnvironmentVariable("SENDGRID_TO")
            ?? throw new InvalidOperationException("TO cannot be empty.");

        _emailProvider = Emails.SendGrid(new SendGridParams(apiKey));
    }

    [Fact]
    public void SendEmailWithEmptyApiKeyShouldThrow()
    {
        var sendGridParamsFunc = () => Emails.SendGrid(new SendGridParams(""));
        sendGridParamsFunc.Should().Throw<ArgumentNullException>("Bearer token cannot be empty.");
    }

    [Fact]
    public async Task SendEmailWithSandboxMode()
    {
        var request = new SendGridMessage
        {
            Subject = _subject,
            From = _from,
            ToAddresses = new List<EmailAddress> { new(_to, "MailEase") },
            Body = "<h1>Hello</h1>",
            SandBoxMode = true
        };

        await _emailProvider.SendEmailAsync(request);
    }

    [Fact]
    public async Task SendEmailWithInvalidSendAt()
    {
        var request = new SendGridMessage
        {
            Subject = _subject,
            From = _from,
            ToAddresses = new List<EmailAddress> { new(_to, "MailEase") },
            Body = "<h1>Hello</h1>",
            SendAt = DateTimeOffset.UtcNow.AddHours(72).AddSeconds(1)
        };

        var sendEmailAsync = async () => await _emailProvider.SendEmailAsync(request);

        await sendEmailAsync
            .Should()
            .ThrowAsync<MailEaseException>()
            .Where(x => x.Errors.Any(y => y.Code == MailEaseErrorCode.InvalidSendAt));
    }
}