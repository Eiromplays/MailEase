namespace MailEase.Providers.Microsoft;

public sealed record AzureCommunicationEmailAddress(string Address, string DisplayName = "")
{
    public override string ToString() =>
        string.IsNullOrWhiteSpace(DisplayName) ? Address : $"{Address} <{DisplayName}>";

    public static implicit operator string(AzureCommunicationEmailAddress address) =>
        address.ToString();

    public static implicit operator AzureCommunicationEmailAddress(string address) => new(address);

    public static implicit operator AzureCommunicationEmailAddress(EmailAddress address) =>
        new(address.Address, address.Name ?? "");
}
