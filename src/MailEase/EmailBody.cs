namespace MailEase;

public class EmailBody
{
    public string Content { get; set; }
    
    public bool IsHtml { get; set; }
    
    public string? PlainTextAlternativeBody { get; set; }
    
    public EmailBody(string content, bool isHtml = false, string? plainTextAlternativeBody = null)
    {
        Content = content;
        IsHtml = isHtml;
        PlainTextAlternativeBody = plainTextAlternativeBody;
    }
}