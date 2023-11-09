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
    public const string From = "YOUR_FROM_EMAIL_HERE";
    public const string To = "YOUR_TO_EMAIL_HERE";

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
            .Where(x => x.Errors.Any(y => y.ErrorCode == MailEaseErrorCode.InvalidSubject));
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
            .Where(x => x.Errors.Any(y => y.ErrorCode == MailEaseErrorCode.InvalidFromAddress));
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
            .Where(x => x.Errors.Any(y => y.ErrorCode == MailEaseErrorCode.NoRecipients));
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
            .Where(x => x.Errors.Any(y => y.ErrorCode == MailEaseErrorCode.InvalidToRecipients));
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
            .Where(x => x.Errors.Any(y => y.ErrorCode == MailEaseErrorCode.InvalidCcRecipients));
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
            .Where(x => x.Errors.Any(y => y.ErrorCode == MailEaseErrorCode.InvalidBccRecipients));
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
                x => x.Errors.Any(y => y.ErrorCode == MailEaseErrorCode.InvalidReplyToRecipients)
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
            .Where(x => x.Errors.Any(y => y.ErrorCode == MailEaseErrorCode.InvalidBody));
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
                            y.ErrorCode == MailEaseErrorCode.InvalidBody
                            || y.ErrorCode == MailEaseErrorCode.InvalidSubject
                    )
            );
    }
}
