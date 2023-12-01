namespace MailEase.Providers.Mailtrap;

public sealed record MailtrapMessage : BaseEmailMessage
{
    public Dictionary<string, string> Headers { get; init; } = new();

    public object? CustomVariables { get; init; }

    public string? Category { get; init; }
}
