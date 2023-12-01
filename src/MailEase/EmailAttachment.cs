namespace MailEase;

public record EmailAttachment(
    string FileName,
    Stream Content,
    string ContentType,
    string? ContentId = null,
    bool IsInline = false
);
