using System.Text.Json;

namespace MailEase.Providers.SendGrid;

public class SendGridErrorResponse
{
    public List<SendGridError> Errors { get; init; } = new();

    public string? Id { get; set; }
}

public sealed class SendGridError
{
    /// <summary>
    /// This is the error message coming from SendGrid it is required, so it should always be set/provided.
    /// </summary>
    public required string Message { get; set; }

    public string? Field { get; set; }

    /// <summary>
    /// The SendGrid documentation specifies this as a <see cref="object"/>
    /// but it seems to actually just be a <see cref="string"/> for the most part
    /// so we're just going to use <see cref="JsonElement"/> so we can access the value no matter the type.
    /// <description>helper text or docs for troubleshooting</description>
    /// </summary>
    public JsonElement? Help { get; set; }
}
