using System.Text;

namespace MailEase.Utils;

public static class StringExtensions
{
    public static string? SHA256(this string? str)
    {
        return str is null ? null : Encoding.UTF8.GetBytes(str).SHA256().ToHexString();
    }
}
