namespace MailEase.Test;

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

    public const string Subject = "MailEase";

    // These do not have to be actual email addresses, as long as they follow the format of an email address.
    public const string From = "sender@example.com";
    public const string To = "yourmail@example.com";

    [Fact]
    public async Task SendEmailWithInvalidSubject()
    {
        var request = new BaseTestEmailMessage
        {
            Subject = "",
            From = From,
            ToAddresses = new List<EmailAddress> { new(To, "MailEase") },
            Body = "<h1>Hello</h1>",
        };

        var sendEmailAsync = async () => await _emailProvider.SendEmailAsync(request);

        await sendEmailAsync
            .Should()
            .ThrowAsync<MailEaseException>()
            .Where(x => x.Errors.Any(y => y.Code == MailEaseErrorCode.InvalidSubject));
    }

    [Fact]
    public async Task SendEmailWithInvalidFrom()
    {
        var request = new BaseTestEmailMessage
        {
            Subject = Subject,
            From = "",
            ToAddresses = new List<EmailAddress> { new(To, "MailEase") },
            Body = "<h1>Hello</h1>",
        };

        var sendEmailAsync = async () => await _emailProvider.SendEmailAsync(request);

        await sendEmailAsync
            .Should()
            .ThrowAsync<MailEaseException>()
            .Where(x => x.Errors.Any(y => y.Code == MailEaseErrorCode.InvalidFromAddress));
    }

    [Fact]
    public async Task SendEmailWithNoRecipients()
    {
        var request = new BaseTestEmailMessage
        {
            Subject = Subject,
            From = From,
            ToAddresses = new List<EmailAddress>(),
            Body = "<h1>Hello</h1>",
        };

        var sendEmailAsync = async () => await _emailProvider.SendEmailAsync(request);

        await sendEmailAsync
            .Should()
            .ThrowAsync<MailEaseException>()
            .Where(x => x.Errors.Any(y => y.Code == MailEaseErrorCode.NoRecipients));
    }

    [Fact]
    public async Task SendEmailWithInvalidToRecipients()
    {
        var request = new BaseTestEmailMessage
        {
            Subject = Subject,
            From = From,
            ToAddresses = new List<EmailAddress> { new("email.com") },
            Body = "<h1>Hello</h1>",
        };

        var sendEmailAsync = async () => await _emailProvider.SendEmailAsync(request);

        await sendEmailAsync
            .Should()
            .ThrowAsync<MailEaseException>()
            .Where(x => x.Errors.Any(y => y.Code == MailEaseErrorCode.InvalidToRecipients));
    }

    [Fact]
    public async Task SendEmailWithInvalidCcRecipients()
    {
        var request = new BaseTestEmailMessage
        {
            Subject = Subject,
            From = From,
            ToAddresses = new List<EmailAddress> { new("myemail@example.com") },
            CcAddresses = new List<EmailAddress> { new("email.com") },
            Body = "<h1>Hello</h1>",
        };

        var sendEmailAsync = async () => await _emailProvider.SendEmailAsync(request);

        await sendEmailAsync
            .Should()
            .ThrowAsync<MailEaseException>()
            .Where(x => x.Errors.Any(y => y.Code == MailEaseErrorCode.InvalidCcRecipients));
    }

    [Fact]
    public async Task SendEmailWithInvalidBccRecipients()
    {
        var request = new BaseTestEmailMessage
        {
            Subject = Subject,
            From = From,
            ToAddresses = new List<EmailAddress> { new("myemail@example.com") },
            BccAddresses = new List<EmailAddress> { new("email.com") },
            Body = "<h1>Hello</h1>",
        };

        var sendEmailAsync = async () => await _emailProvider.SendEmailAsync(request);

        await sendEmailAsync
            .Should()
            .ThrowAsync<MailEaseException>()
            .Where(x => x.Errors.Any(y => y.Code == MailEaseErrorCode.InvalidBccRecipients));
    }

    [Fact]
    public async Task SendEmailWithInvalidReplyToRecipients()
    {
        var request = new BaseTestEmailMessage
        {
            Subject = Subject,
            From = From,
            ToAddresses = new List<EmailAddress> { new("myemail@example.com") },
            ReplyToAddresses = new List<EmailAddress> { new("email.com") },
            Body = "<h1>Hello</h1>",
        };

        var sendEmailAsync = async () => await _emailProvider.SendEmailAsync(request);

        await sendEmailAsync
            .Should()
            .ThrowAsync<MailEaseException>()
            .Where(
                x => x.Errors.Any(y => y.Code == MailEaseErrorCode.InvalidReplyToRecipients)
            );
    }

    [Fact]
    public async Task SendEmailWithInvalidBody()
    {
        var request = new BaseTestEmailMessage
        {
            Subject = Subject,
            From = From,
            ToAddresses = new List<EmailAddress> { new("myemail@example.com") },
            ReplyToAddresses = new List<EmailAddress> { new("email.com") },
            Body = "",
        };

        var sendEmailAsync = async () => await _emailProvider.SendEmailAsync(request);

        await sendEmailAsync
            .Should()
            .ThrowAsync<MailEaseException>()
            .Where(x => x.Errors.Any(y => y.Code == MailEaseErrorCode.InvalidBody));
    }

    [Fact]
    public async Task SendEmailWithInvalidBodyAndSubject()
    {
        var request = new BaseTestEmailMessage
        {
            Subject = "",
            From = From,
            ToAddresses = new List<EmailAddress> { new("myemail@example.com") },
            Body = "",
        };

        var sendEmailAsync = async () => await _emailProvider.SendEmailAsync(request);

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
