namespace DevZest.Data
{
    public interface IValidator
    {
        ValidationSeverity Severity { get; }
        ValidationMessage Validate(DataRow dataRow);
    }
}
