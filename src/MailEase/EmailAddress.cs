namespace MailEase;

public sealed class EmailAddress
{
    public string Address { get; set; }

    public string? Name { get; set; }

    public EmailAddress(string address, string? name = null)
    {
        Address = address;
        Name = name;
    }
    
    public override string ToString() =>
        string.IsNullOrWhiteSpace(Name) ? Address : $"{Name} <{Address}>";
}