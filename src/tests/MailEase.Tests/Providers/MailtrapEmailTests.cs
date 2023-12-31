using System.Text;
using MailEase.Providers.Mailtrap;
using MailEase.Tests.Fakes;
using Microsoft.Extensions.Configuration;

namespace MailEase.Tests.Providers;

public sealed class MailtrapEmailTests : IClassFixture<ConfigurationFixture>
{
    private readonly IEmailProvider<MailtrapMessage> _emailProvider;
    private readonly string _subject = "MailEase";
    private readonly string _from;
    private readonly string _to;

    public MailtrapEmailTests(ConfigurationFixture fixture)
    {
        var config = fixture.Config;

        var apiKey =
            config.GetValue<string>("MAILTRAP_API_KEY")
            ?? throw new InvalidOperationException("Mailtrap API key cannot be empty.");
        var inboxId =
            config.GetValue<int?>("MAILTRAP_INBOX_ID")
            ?? throw new InvalidOperationException("Inbox ID cannot be empty.");

        _subject = config.GetValue<string>("MAILTRAP_SUBJECT") ?? _subject;
        _from =
            config.GetValue<string>("MAILTRAP_FROM")
            ?? throw new InvalidOperationException("FROM cannot be empty.");
        _to =
            config.GetValue<string>("MAILTRAP_TO")
            ?? throw new InvalidOperationException("TO cannot be empty.");
        // TODO: Implement/use this when I get access to the MailTrap sandbox mode
        _emailProvider = Emails.Mailtrap(
            new MailtrapParams(apiKey, "https://sandbox.api.mailtrap.io", $"api/send/{inboxId}")
        );
        //_emailProvider = Emails.Mailtrap(new MailtrapParams(apiKey));
    }

    [Fact]
    public void SendEmail_WithEmptyApiKey_ShouldThrowArgumentNullException()
    {
        var mailtrapParamsFunc = () => Emails.Mailtrap(new MailtrapParams(""));
        mailtrapParamsFunc.Should().Throw<ArgumentNullException>("Bearer token cannot be empty.");
    }

    [Fact]
    public Task SendEmail_ShouldSucceed()
    {
        var request = new MailtrapMessage
        {
            Subject = _subject,
            From = new EmailAddress(_from, "MailEase"),
            ToAddresses = new List<EmailAddress> { new(_to, "MailEase") },
            Html = "<h1>Hello</h1>"
        };

        return _emailProvider.SendEmailAsync(request);
    }

    [Fact]
    public Task SendEmail_WithAttachment_ShouldSucceed()
    {
        var attachment = new EmailAttachment(
            "MyVerySecretAttachment.txt",
            new MemoryStream(Encoding.UTF8.GetBytes("Hello World!")),
            "text/plain"
        );

        var request = new MailtrapMessage
        {
            Subject = _subject,
            From = new EmailAddress(_from, "MailEase"),
            ToAddresses = new List<EmailAddress> { new(_to, "MailEase") },
            Attachments = new List<EmailAttachment> { attachment },
            Html = "<h1>Hello</h1>"
        };

        return _emailProvider.SendEmailAsync(request);
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

        var request = new MailtrapMessage
        {
            Subject = _subject,
            From = _from,
            ToAddresses = toAddresses,
            CcAddresses = ccAddresses,
            BccAddresses = bccAddresses,
            Html = "<h1>Hello</h1>"
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

        var request = new MailtrapMessage
        {
            Subject = _subject,
            From = _from,
            ToAddresses = toAddresses,
            CcAddresses = ccAddresses,
            BccAddresses = bccAddresses,
            Html = "<h1>Hello</h1>",
            UseSplitting = false
        };

        var sendEmailAsync = () => _emailProvider.SendEmailAsync(request);

        await sendEmailAsync
            .Should()
            .ThrowAsync<MailEaseException>()
            .Where(x => x.Errors.Any(y => y.Code == MailEaseErrorCode.RecipientsExceedLimit));
    }
}
