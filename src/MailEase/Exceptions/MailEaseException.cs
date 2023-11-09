namespace MailEase.Exceptions;

public class MailEaseException : Exception
{
    private readonly List<MailEaseErrorDetail> _errors = new();

    public IReadOnlyList<MailEaseErrorDetail> Errors => _errors;

    public void AddError(MailEaseErrorDetail error)
    {
        _errors.Add(error);
    }

    public void AddErrors(IEnumerable<MailEaseErrorDetail> errors)
    {
        _errors.AddRange(errors);
    }
}
