using MailEase.Providers.SendGrid;
using Xunit.Abstractions;

namespace MailEase.Test.Providers;

public sealed class SendGridTests
{
    private readonly IEmailProvider<SendGridMessage> _emailProvider;
    private readonly ITestOutputHelper _testOutputHelper;

    public SendGridTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        var apiKey =
            Environment.GetEnvironmentVariable("SENDGRID_API_KEY") ?? "YOUR_SENDGRID_API_KEY";

        _emailProvider = Emails.SendGrid(new SendGridParams(apiKey));
    }

    public const string Subject = "MailEase";
    public const string From = "YOUR_FROM_EMAIL_HERE";
    public const string To = "YOUR_TO_EMAIL_HERE";

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
            Subject = Subject,
            From = From,
            ToAddresses = new List<EmailAddress> { new(To, "MailEase") },
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
            Subject = Subject,
            From = From,
            ToAddresses = new List<EmailAddress> { new(To, "MailEase") },
            Body = "<h1>Hello</h1>",
            SendAt = DateTimeOffset.UtcNow.AddHours(72).AddSeconds(1)
        };

        var sendEmailAsync = async () => await _emailProvider.SendEmailAsync(request);

        await sendEmailAsync
            .Should()
            .ThrowAsync<MailEaseException>()
            .Where(x => x.Errors.Any(y => y.ErrorCode == MailEaseErrorCode.InvalidSendAt));
    }
}
