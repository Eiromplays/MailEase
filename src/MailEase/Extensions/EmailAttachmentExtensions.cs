namespace MailEase.Extensions;

public static class EmailAttachmentExtensions
{
    public static async Task<byte[]> ToByteArrayAsync(this EmailAttachment attachment, CancellationToken cancellationToken = default)
    {
        await using var memoryStream = new MemoryStream();
        await attachment.Data.CopyToAsync(memoryStream, cancellationToken);
        return memoryStream.ToArray();
    }
}