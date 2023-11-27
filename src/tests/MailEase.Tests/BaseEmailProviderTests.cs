namespace MailEase.Tests;

/// <summary>
/// A email provider used for testing
/// </summary>
internal sealed class BaseTestEmailProvider : BaseEmailProvider<BaseTestEmailMessage>
{
    public override Task SendEmailAsync(
        BaseTestEmailMessage message,
        CancellationToken cancellationToken = default
    )
    {
        ValidateEmailMessage(message);

        return Task.CompletedTask;
    }
}

/// <summary>
/// A email message used for testing
/// </summary>
internal sealed record BaseTestEmailMessage : BaseEmailMessage;

public sealed class BaseEmailProviderTests
{
    private readonly IEmailProvider<BaseTestEmailMessage> _emailProvider =
        new BaseTestEmailProvider();

    private const string Subject = "MailEase";

    // These do not have to be actual email addresses, as long as they follow the format of an email address.
    private const string From = "sender@example.com";
    private const string To = "yourmail@example.com";

    [Fact]
    public async Task SendEmail_WithInvalidSubject_ShouldThrowMailEaseException()
    {
        var request = new BaseTestEmailMessage
        {
            Subject = "",
            From = From,
            ToAddresses = new List<EmailAddress> { new(To, "MailEase") },
            Html = "<h1>Hello</h1>",
        };

        var sendEmailAsync = () => _emailProvider.SendEmailAsync(request);

        await sendEmailAsync
            .Should()
            .ThrowAsync<MailEaseException>()
            .Where(x => x.Errors.Any(y => y.Code == MailEaseErrorCode.InvalidSubject));
    }

    [Fact]
    public async Task SendEmail_WithInvalidFrom_ShouldThrowMailEaseException()
    {
        var request = new BaseTestEmailMessage
        {
            Subject = Subject,
            From = "",
            ToAddresses = new List<EmailAddress> { new(To, "MailEase") },
            Html = "<h1>Hello</h1>",
        };

        var sendEmailAsync = () => _emailProvider.SendEmailAsync(request);

        await sendEmailAsync
            .Should()
            .ThrowAsync<MailEaseException>()
            .Where(x => x.Errors.Any(y => y.Code == MailEaseErrorCode.InvalidFromAddress));
    }

    [Fact]
    public async Task SendEmail_WithNoRecipients_ShouldThrowMailEaseException()
    {
        var request = new BaseTestEmailMessage
        {
            Subject = Subject,
            From = From,
            ToAddresses = new List<EmailAddress>(),
            Html = "<h1>Hello</h1>",
        };

        var sendEmailAsync = () => _emailProvider.SendEmailAsync(request);

        await sendEmailAsync
            .Should()
            .ThrowAsync<MailEaseException>()
            .Where(x => x.Errors.Any(y => y.Code == MailEaseErrorCode.NoRecipients));
    }

    [Fact]
    public async Task SendEmail_WithInvalidToRecipient_ShouldThrowMailEaseException()
    {
        var request = new BaseTestEmailMessage
        {
            Subject = Subject,
            From = From,
            ToAddresses = new List<EmailAddress> { new("email.com") },
            Html = "<h1>Hello</h1>",
        };

        var sendEmailAsync = () => _emailProvider.SendEmailAsync(request);

        await sendEmailAsync
            .Should()
            .ThrowAsync<MailEaseException>()
            .Where(x => x.Errors.Any(y => y.Code == MailEaseErrorCode.InvalidToRecipients));
    }

    [Fact]
    public async Task SendEmail_WithInvalidCcRecipient_ShouldThrowMailEaseException()
    {
        var request = new BaseTestEmailMessage
        {
            Subject = Subject,
            From = From,
            ToAddresses = new List<EmailAddress> { new("myemail@example.com") },
            CcAddresses = new List<EmailAddress> { new("email.com") },
            Html = "<h1>Hello</h1>",
        };

        var sendEmailAsync = () => _emailProvider.SendEmailAsync(request);

        await sendEmailAsync
            .Should()
            .ThrowAsync<MailEaseException>()
            .Where(x => x.Errors.Any(y => y.Code == MailEaseErrorCode.InvalidCcRecipients));
    }

    [Fact]
    public async Task SendEmail_WithInvalidBccRecipient_ShouldThrowMailEaseException()
    {
        var request = new BaseTestEmailMessage
        {
            Subject = Subject,
            From = From,
            ToAddresses = new List<EmailAddress> { new("myemail@example.com") },
            BccAddresses = new List<EmailAddress> { new("email.com") },
            Html = "<h1>Hello</h1>",
        };

        var sendEmailAsync = () => _emailProvider.SendEmailAsync(request);

        await sendEmailAsync
            .Should()
            .ThrowAsync<MailEaseException>()
            .Where(x => x.Errors.Any(y => y.Code == MailEaseErrorCode.InvalidBccRecipients));
    }

    [Fact]
    public async Task SendEmail_WithInvalidReplyToRecipient_ShouldThrowMailEaseException()
    {
        var request = new BaseTestEmailMessage
        {
            Subject = Subject,
            From = From,
            ToAddresses = new List<EmailAddress> { new("myemail@example.com") },
            ReplyToAddresses = new List<EmailAddress> { new("email.com") },
            Html = "<h1>Hello</h1>",
        };

        var sendEmailAsync = () => _emailProvider.SendEmailAsync(request);

        await sendEmailAsync
            .Should()
            .ThrowAsync<MailEaseException>()
            .Where(x => x.Errors.Any(y => y.Code == MailEaseErrorCode.InvalidReplyToRecipients));
    }

    [Fact]
    public async Task SendEmail_WithInvalidBody_ShouldThrowMailEaseException()
    {
        var request = new BaseTestEmailMessage
        {
            Subject = Subject,
            From = From,
            ToAddresses = new List<EmailAddress> { new("myemail@example.com") },
            ReplyToAddresses = new List<EmailAddress> { new("email.com") },
        };

        var sendEmailAsync = () => _emailProvider.SendEmailAsync(request);

        await sendEmailAsync
            .Should()
            .ThrowAsync<MailEaseException>()
            .Where(x => x.Errors.Any(y => y.Code == MailEaseErrorCode.InvalidBody));
    }

    [Fact]
    public async Task SendEmail_WithInvalidBodyAndSubject_ShouldThrowMailEaseException()
    {
        var request = new BaseTestEmailMessage
        {
            Subject = "",
            From = From,
            ToAddresses = new List<EmailAddress> { new("myemail@example.com") },
        };

        var sendEmailAsync = () => _emailProvider.SendEmailAsync(request);

        await sendEmailAsync
            .Should()
            .ThrowAsync<MailEaseException>()
            .Where(
                x =>
                    x.Errors.Count == 2
                    && x.Errors.All(
                        y =>
                            y.Code == MailEaseErrorCode.InvalidBody
                            || y.Code == MailEaseErrorCode.InvalidSubject
                    )
            );
    }
}
