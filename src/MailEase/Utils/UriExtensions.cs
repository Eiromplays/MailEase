namespace MailEase.Utils;

public static class UriExtensions
{
    public static string GetAbsolutePathUnencoded(this Uri uri)
    {
        var result = uri.ToString();

        // remove protocol
        var i = result.IndexOf("://", StringComparison.Ordinal);
        if (i >= 0)
            result = result[(i + 3)..];

        // remove host and port
        i = result.IndexOf('/');
        if (i >= 0)
            result = result[i..];

        // remove query string
        i = result.IndexOf('?');
        if (i >= 0)
            result = result[..i];

        return result;
    }
}
