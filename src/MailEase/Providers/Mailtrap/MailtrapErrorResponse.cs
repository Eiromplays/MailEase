namespace MailEase.Providers.Mailtrap;

public sealed class MailtrapErrorResponse
{
    public bool Success { get; init; }

    public List<string> Errors { get; init; } = new();
}
