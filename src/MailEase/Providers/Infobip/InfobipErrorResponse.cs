using System.Text.Json;
using System.Text.Json.Serialization;

namespace MailEase.Providers.Infobip;

public sealed class InfobipErrorResponse
{
    [JsonPropertyName("requestError")]
    public required InfobipRequestError RequestError { get; init; }
}

public sealed class InfobipRequestError
{
    [JsonPropertyName("serviceException")]
    public required InfobipServiceException ServiceException { get; init; }
}

public sealed class InfobipServiceException
{
    [JsonPropertyName("messageId")]
    public string MessageId { get; init; } = default!;

    [JsonPropertyName("text")]
    public required string Text { get; init; }

    public List<InfobipValidationError> ValidationErrors { get; init; } = [];
}

public sealed class InfobipValidationError
{
    public string Field { get; init; } = default!;
    
    public string Message { get; init; } = default!;
}