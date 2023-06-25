namespace MailEase;

public class EmailAttachment
{
    public string FileName { get; set; }
    
    public string ContentType { get; set; }
    
    public Stream Data { get; set; }

    public bool IsInline { get; set; }
    
    public string? ContentId { get; set; }
    
    public EmailAttachment(string fileName, string contentType, Stream data, bool isInline = false, string? contentId = null)
    {
        FileName = fileName;
        ContentType = contentType;
        Data = data;
        IsInline = isInline;
        ContentId = contentId;
    }
}