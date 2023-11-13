namespace MailEase;

public record EmailAddress(string Address, string? Name = null)
{
    public override string ToString() =>
        string.IsNullOrWhiteSpace(Name) ? Address : $"{Address} <{Name}>";

    public static implicit operator string(EmailAddress address) => address.ToString();

    public static implicit operator EmailAddress(string address) => new(address);

    public bool IsValid
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Address))
                return false;

            // only return true if there is only 1 '@' character
            // and it is neither the first nor the last character
            var index = Address.IndexOf('@');

            return index > 0 && index != Address.Length - 1 && index == Address.LastIndexOf('@');
        }
    }
}
