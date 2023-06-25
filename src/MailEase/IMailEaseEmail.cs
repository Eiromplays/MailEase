namespace MailEase;

/// <summary>
/// Represents an email.
/// </summary>
public interface IMailEaseEmail : ICanSetFromAddress, ICanSetToAddress, ICanSetSubject, ICanSetBody, ICanSetProperties
{
    EmailData Data { get; }

    IEmailSender Sender { get; }
}

/// <summary>
/// Represents the from address step in the email builder.
/// </summary>
public interface ICanSetFromAddress
{
    ICanSetToAddress From(EmailAddress from);
    
    ICanSetToAddress From(string address, string? name = null) =>
        From(new EmailAddress(address, name));
}

/// <summary>
/// Represents the subject step in the email builder.
/// </summary>
public interface ICanSetSubject
{
    ICanSetBody Subject(string subject);
}

/// <summary>
/// Represents the body step in the email builder.
/// </summary>
public interface ICanSetBody
{
    ICanSetProperties Body(EmailBody body);
    
    ICanSetProperties Body(string body) =>
        Body(new EmailBody(body));
    
    ICanSetProperties Body(string body, bool isHtml) =>
        Body(new EmailBody(body, isHtml));
}

/// <summary>
/// Represents the to address step in the email builder.
/// </summary>
public interface ICanSetToAddress
{
    ICanSetSubject To(EmailAddress to);
    
    ICanSetSubject To(string address, string? name = null) =>
        To(new EmailAddress(address, name));
    
    ICanSetSubject To(IEnumerable<EmailAddress> to);
    
    ICanSetSubject To(params EmailAddress[] to) =>
        To((IEnumerable<EmailAddress>) to);
    
    ICanSetSubject To(params string[] to) =>
        To(to.Select(x => new EmailAddress(x)));
    
    ICanSetSubject To(IEnumerable<string> to) =>
        To(to.Select(x => new EmailAddress(x)));
}

/// <summary>
/// Represents the properties step in the email builder.
/// </summary>
public interface ICanSetProperties : ICanSetCc, ICanSetBcc, ICanSetReplyTo, ICanSetAttachments, ICanSetHeaders, ICanSetTags, ICanSetVariables, ICanSetPriority, ICanSetSandboxMode, ICanBuild
{
    
}

/// <summary>
/// Represents the cc step in the email builder.
/// </summary>
public interface ICanSetCc
{
    ICanSetProperties Cc(EmailAddress to);
    
    ICanSetProperties Cc(string address, string? name = null) =>
        Cc(new EmailAddress(address, name));
    
    ICanSetProperties Cc(IEnumerable<EmailAddress> to);
    
    ICanSetProperties Cc(params EmailAddress[] to) =>
        Cc((IEnumerable<EmailAddress>) to);
    
    ICanSetProperties Cc(params string[] to) =>
        Cc(to.Select(x => new EmailAddress(x)));
    
    ICanSetProperties Cc(IEnumerable<string> to) =>
        Cc(to.Select(x => new EmailAddress(x)));
}

/// <summary>
/// Represents the bcc step in the email builder.
/// </summary>
public interface ICanSetBcc
{
    ICanSetProperties Bcc(EmailAddress to);
    
    ICanSetProperties Bcc(string address, string? name = null) =>
        Bcc(new EmailAddress(address, name));
    
    ICanSetProperties Bcc(IEnumerable<EmailAddress> to);
    
    ICanSetProperties Bcc(params EmailAddress[] to) =>
        Bcc((IEnumerable<EmailAddress>) to);
    
    ICanSetProperties Bcc(params string[] to) =>
        Bcc(to.Select(x => new EmailAddress(x)));
    
    ICanSetProperties Bcc(IEnumerable<string> to) =>
        Bcc(to.Select(x => new EmailAddress(x)));
}

/// <summary>
/// Represents the reply to step in the email builder.
/// </summary>
public interface ICanSetReplyTo
{
    ICanSetProperties ReplyTo(EmailAddress to);
    
    ICanSetProperties ReplyTo(string address, string? name = null) =>
        ReplyTo(new EmailAddress(address, name));
    
    ICanSetProperties ReplyTo(IEnumerable<EmailAddress> to);
    
    ICanSetProperties ReplyTo(params EmailAddress[] to) =>
        ReplyTo((IEnumerable<EmailAddress>) to);
    
    ICanSetProperties ReplyTo(params string[] to) =>
        ReplyTo(to.Select(x => new EmailAddress(x)));
    
    ICanSetProperties ReplyTo(IEnumerable<string> to) =>
        ReplyTo(to.Select(x => new EmailAddress(x)));
}

/// <summary>
/// Represents the attachments step in the email builder.
/// </summary>
public interface ICanSetAttachments
{
    ICanSetProperties Attachments(IEnumerable<EmailAttachment> attachments);
    
    ICanSetProperties Attachments(params EmailAttachment[] attachments) =>
        Attachments((IEnumerable<EmailAttachment>) attachments);
}

/// <summary>
/// Represents the headers step in the email builder.
/// </summary>
public interface ICanSetHeaders
{
    ICanSetProperties Headers(Dictionary<string, string> headers);
    
    ICanSetProperties Headers(params (string, string)[] headers) =>
        Headers(headers.ToDictionary(x => x.Item1, x => x.Item2));
}

/// <summary>
/// Represents the tags step in the email builder.
/// </summary>
public interface ICanSetTags
{
    ICanSetProperties Tags(IEnumerable<string> tags);

    ICanSetProperties Tags(params string[] tags) =>
        Tags((IEnumerable<string>)tags);
}

/// <summary>
/// Represents the variables step in the email builder.
/// This is currently only supported by the Mailgun provider. <see href="https://documentation.mailgun.com/en/latest/user_manual.html#attaching-data-to-messages"/>
/// </summary>
public interface ICanSetVariables
{
    /// <summary>
    /// Adds a variables to the email.
    /// This is currently only supported by the Mailgun provider. <see href="https://documentation.mailgun.com/en/latest/user_manual.html#attaching-data-to-messages"/>
    /// </summary>
    /// <param name="variables">Variables to add.</param>
    /// <returns>New email builder step.</returns>
    ICanSetProperties Variables(Dictionary<string, string> variables);

    /// <summary>
    /// Adds a variables to the email.
    /// This is currently only supported by the Mailgun provider. <see href="https://documentation.mailgun.com/en/latest/user_manual.html#attaching-data-to-messages"/>
    /// </summary>
    /// <param name="variables">Variables to add.</param>
    /// <returns>New email builder step.</returns>
    ICanSetProperties Variables(params (string, string)[] variables) =>
        Variables(variables.ToDictionary(x => x.Item1, x => x.Item2));
}

/// <summary>
/// Represents the priority step in the email builder.
/// </summary>
public interface ICanSetPriority
{
    ICanSetProperties Priority(EmailPriority priority);
}

/// <summary>
/// Represents the sandbox mode step in the email builder.
/// </summary>
public interface ICanSetSandboxMode
{
    ICanSetProperties SandboxMode(bool sandboxMode);
}

/// <summary>
/// Represents the build step in the email builder. This is the final step.
/// </summary>
public interface ICanBuild
{
    IMailEaseEmail Build();
}