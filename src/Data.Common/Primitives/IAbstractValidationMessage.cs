namespace DevZest.Data.Primitives
{
    public interface IAbstractValidationMessage
    {
        string Id { get; }
        string Description { get; }
        ValidationSeverity Severity { get; }
    }
}
