using System.Text.RegularExpressions;

namespace MailEase;

public record EmailAddress(string Address, string? Name = null)
{
    public override string ToString() =>
        string.IsNullOrWhiteSpace(Name) ? Address : $"{Address} <{Name}>";

    public static implicit operator string(EmailAddress address) => address.ToString();

    public static implicit operator EmailAddress(string address) => new(address);

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(Address)
        && Regex.IsMatch(Address, @"^[\w-]+(\.[\w-]+)*@([\w-]+\.)+[a-zA-Z]{2,7}$");
}
