namespace MailEase.Providers.Infobip;

public sealed class InfobipResponse
{
    public required string BulkId { get; init; }

    public List<InfobipResponseMessage> Messages { get; init; } = new();
}

public sealed class InfobipResponseMessage
{
    public required string To { get; init; }
    public required string MessageId { get; init; }

    public required InfobipResponseMessageStatus Status { get; init; }
}

public sealed class InfobipResponseMessageStatus
{
    public int Id { get; init; }

    public int GroupId { get; init; }

    public required string GroupName { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }
}
