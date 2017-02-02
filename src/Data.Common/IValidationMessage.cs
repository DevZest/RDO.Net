namespace DevZest.Data
{
    public interface IValidationMessage
    {
        string Id { get; }
        string Description { get; }
        ValidationSeverity Severity { get; }
    }
}
