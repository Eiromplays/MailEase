using MailEase.Default;
using MailEase.Extensions;

namespace MailEase;

public class Email : IMailEaseEmail
{
    public EmailData Data { get; private set; } = new();

    public IEmailSender Sender { get; private set; } = new SaveToDiskEmailSender();

    private Email() {}

    internal Email(IEmailSender sender)
    {
        Sender = sender;
    }
    
    internal Email(EmailAddress from, IEmailSender sender)
    {
        Sender = sender;
        Data.From = from;
    }
    
    public static IMailEaseEmail CreateBuilder() =>
        new Email();

    ICanSetToAddress ICanSetFromAddress.From(EmailAddress from)
    {
        Data.From = from;
        return this;
    }

    ICanSetSubject ICanSetToAddress.To(EmailAddress to)
    {
        Data.To.Add(to);
        return this;
    }

    ICanSetSubject ICanSetToAddress.To(IEnumerable<EmailAddress> to)
    {
        Data.To.AddRange(to);
        return this;
    }

    ICanSetBody ICanSetSubject.Subject(string subject)
    {
        Data.Subject = subject;
        return this;
    }

    ICanSetProperties ICanSetBody.Body(EmailBody body)
    {
        Data.Body = body;
        return this;
    }

    ICanSetProperties ICanSetCc.Cc(EmailAddress to)
    {
        Data.Cc.Add(to);
        return this;
    }

    ICanSetProperties ICanSetCc.Cc(IEnumerable<EmailAddress> to)
    {
        Data.Cc.AddRange(to);
        return this;
    }

    ICanSetProperties ICanSetBcc.Bcc(EmailAddress to)
    {
        Data.Bcc.Add(to);
        return this;
    }

    ICanSetProperties ICanSetBcc.Bcc(IEnumerable<EmailAddress> to)
    {
        Data.Bcc.AddRange(to);
        return this;
    }

    ICanSetProperties ICanSetReplyTo.ReplyTo(EmailAddress to)
    {
        Data.ReplyTo.Add(to);
        return this;
    }

    ICanSetProperties ICanSetReplyTo.ReplyTo(IEnumerable<EmailAddress> to)
    {
        Data.ReplyTo.AddRange(to);
        return this;
    }

    ICanSetProperties ICanSetAttachments.Attachments(IEnumerable<EmailAttachment> attachments)
    {
        Data.Attachments.AddRange(attachments);
        return this;
    }

    ICanSetProperties ICanSetHeaders.Headers(Dictionary<string, string> headers)
    {
        Data.Headers = headers;
        return this;
    }
    
    ICanSetProperties ICanSetTags.Tags(IEnumerable<string> tags)
    {
        Data.Tags.AddRange(tags);
        return this;
    }
    
    ICanSetProperties ICanSetVariables.Variables(Dictionary<string, string> variables)
    {
        Data.Variables = variables;
        return this;
    }

    ICanSetProperties ICanSetPriority.Priority(EmailPriority priority)
    {
        Data.Priority = priority;
        return this;
    }
    
    ICanSetProperties ICanSetSandboxMode.SandboxMode(bool isSandboxMode)
    {
        Data.IsSandboxMode = isSandboxMode;
        return this;
    }

    IMailEaseEmail ICanBuild.Build() => this;
}