namespace MailEase;

public class EmailData
{
    public EmailAddress From { get; set; } = null!;
    
    public string Subject { get; set; } = null!;
    
    public EmailBody Body { get; set; } = null!;
    
    public EmailPriority Priority { get; set; }
    
    public IList<EmailAddress> To { get; } = new List<EmailAddress>();
    
    public IList<EmailAddress> Cc { get; } = new List<EmailAddress>();
    
    public IList<EmailAddress> Bcc { get; } = new List<EmailAddress>();
    
    public IList<EmailAddress> ReplyTo { get; } = new List<EmailAddress>();

    public IList<EmailAttachment> Attachments { get; } = new List<EmailAttachment>();
    
    public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
    
    public IList<string> Tags { get; set; } = new List<string>();
    
    public IDictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// This property is used to enable/disable the sandbox mode also known as test mode.
    /// </summary>
    public bool IsSandboxMode { get; set; }
}