using System.Text.Json;
using MailEase.Results;

namespace MailEase.Default;

public class SaveToDiskEmailSender : IEmailSender
{
    public async Task<SendEmailResult> SendAsync(IMailEaseEmail email, CancellationToken cancellationToken = default)
    {
        var content = JsonSerializer.Serialize(email, new JsonSerializerOptions { WriteIndented = true });
        
        var sendEmailResult = new SendEmailResult<string> { Data = content };
        
        await SaveToDiskAsync(content, cancellationToken);
        
        return sendEmailResult;
    }
    
    private async Task SaveToDiskAsync(string content, CancellationToken cancellationToken = default)
    {
        var fileName = $"{Guid.NewGuid()}.json";
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
        await File.WriteAllTextAsync(filePath, content, cancellationToken);
    }
}