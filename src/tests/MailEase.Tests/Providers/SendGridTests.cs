using System.Text;
using MailEase.Providers.SendGrid;
using MailEase.Tests.Fakes;
using Microsoft.Extensions.Configuration;

namespace MailEase.Tests.Providers;

public sealed class SendGridTests : IClassFixture<ConfigurationFixture>
{
    private readonly IEmailProvider<SendGridMessage> _emailProvider;
    private readonly string _subject = "MailEase";
    private readonly string _from;
    private readonly string _to;

    public SendGridTests(ConfigurationFixture fixture)
    {
        var config = fixture.Config;

        var apiKey =
            config.GetValue<string>("SENDGRID_API_KEY")
            ?? throw new InvalidOperationException("SendGrid API key cannot be empty.");

        _subject = config.GetValue<string>("SENDGRID_SUBJECT") ?? _subject;
        _from =
            config.GetValue<string>("SENDGRID_FROM")
            ?? throw new InvalidOperationException("FROM cannot be empty.");
        _to =
            config.GetValue<string>("SENDGRID_TO")
            ?? throw new InvalidOperationException("TO cannot be empty.");

        _emailProvider = Emails.SendGrid(new SendGridParams(apiKey));
    }

    [Fact]
    public void SendEmail_WithEmptyApiKey_ShouldThrowArgumentNullException()
    {
        var sendGridParamsFunc = () => Emails.SendGrid(new SendGridParams(""));
        sendGridParamsFunc.Should().Throw<ArgumentNullException>("Bearer token cannot be empty.");
    }

    [Fact]
    public Task SendEmail_WithSandboxMode_ShouldSucceed()
    {
        var request = new SendGridMessage
        {
            Subject = _subject,
            From = _from,
            ToAddresses = new List<EmailAddress> { new(_to, "MailEase") },
            Html = "<h1>Hello</h1>",
            MailSettings = new SendGridMailSettings
            {
                SandBoxMode = new SendGridSandBoxMode { Enable = true }
            }
        };

        return _emailProvider.SendEmailAsync(request);
    }

    [Fact]
    public Task SendEmail_WithSandBoxModeAndAttachment_ShouldSucceed()
    {
        var attachment = new EmailAttachment(
            "MyVerySecretAttachment.txt",
            new MemoryStream(Encoding.UTF8.GetBytes("Hello World!")),
            "text/plain"
        );

        var request = new SendGridMessage
        {
            Subject = _subject,
            From = _from,
            ToAddresses = new List<EmailAddress> { new(_to, "MailEase") },
            Attachments = new List<EmailAttachment> { attachment },
            Html = "<h1>Hello</h1>",
            MailSettings = new SendGridMailSettings
            {
                SandBoxMode = new SendGridSandBoxMode { Enable = true }
            }
        };

        return _emailProvider.SendEmailAsync(request);
    }

    [Fact]
    public async Task SendEmail_WithInvalidSendAt_ShouldThrowMailEaseException()
    {
        var request = new SendGridMessage
        {
            Subject = _subject,
            From = _from,
            ToAddresses = new List<EmailAddress> { new(_to, "MailEase") },
            Html = "<h1>Hello</h1>",
            SendAt = DateTimeOffset.UtcNow.AddHours(72).AddSeconds(1)
        };

        var sendEmailAsync = () => _emailProvider.SendEmailAsync(request);

        await sendEmailAsync
            .Should()
            .ThrowAsync<MailEaseException>()
            .Where(x => x.Errors.Any(y => y.Code == MailEaseErrorCode.InvalidSendAt));
    }

    [Fact]
    public Task SendEmail_With1500RecipientsAndUseSplitting_ShouldSucceed()
    {
        const int totalRecipients = 1500;
        const int recipientSize = totalRecipients / 3;

        var uniqueEmails =
            new FakeEmailAddress().Generate(totalRecipients) ?? new List<EmailAddress>();
        var toAddresses = uniqueEmails.GetRange(0, recipientSize);
        var ccAddresses = uniqueEmails.GetRange(recipientSize, recipientSize);
        var bccAddresses = uniqueEmails.GetRange(recipientSize * 2, recipientSize);

        var request = new SendGridMessage
        {
            Subject = _subject,
            From = _from,
            ToAddresses = toAddresses,
            CcAddresses = ccAddresses,
            BccAddresses = bccAddresses,
            Html = "<h1>Hello</h1>",
            MailSettings = new SendGridMailSettings
            {
                SandBoxMode = new SendGridSandBoxMode { Enable = true }
            }
        };

        return _emailProvider.SendEmailAsync(request);
    }

    [Fact]
    public async Task SendEmail_With1500RecipientsAndWithoutUseSplitting_ShouldThrowMailEaseException()
    {
        const int totalRecipients = 1500;
        const int recipientSize = totalRecipients / 3;

        var uniqueEmails =
            new FakeEmailAddress().Generate(totalRecipients) ?? new List<EmailAddress>();
        var toAddresses = uniqueEmails.GetRange(0, recipientSize);
        var ccAddresses = uniqueEmails.GetRange(recipientSize, recipientSize);
        var bccAddresses = uniqueEmails.GetRange(recipientSize * 2, recipientSize);

        var request = new SendGridMessage
        {
            Subject = _subject,
            From = _from,
            ToAddresses = toAddresses,
            CcAddresses = ccAddresses,
            BccAddresses = bccAddresses,
            Html = "<h1>Hello</h1>",
            MailSettings = new SendGridMailSettings
            {
                SandBoxMode = new SendGridSandBoxMode { Enable = true }
            },
            UseSplitting = false
        };

        var sendEmailAsync = () => _emailProvider.SendEmailAsync(request);

        await sendEmailAsync
            .Should()
            .ThrowAsync<MailEaseException>()
            .Where(x => x.Errors.Any(y => y.Code == MailEaseErrorCode.RecipientsExceedLimit));
    }
}
