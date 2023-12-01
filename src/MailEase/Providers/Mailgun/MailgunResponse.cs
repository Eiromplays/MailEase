namespace MailEase.Providers.Mailgun;

public sealed class MailgunResponse
{
    public required string Id { get; init; }
    
    public required string Message { get; init; }
}