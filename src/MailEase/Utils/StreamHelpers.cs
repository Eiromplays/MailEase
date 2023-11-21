namespace MailEase.Utils;

public static class StreamHelpers
{
    public static async Task<string> StreamToBase64Async(Stream stream)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return Convert.ToBase64String(ms.ToArray());
    }
}
