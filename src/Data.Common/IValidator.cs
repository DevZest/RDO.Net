namespace DevZest.Data
{
    /// <summary></summary>
    public interface IValidator
    {
        string MessageId { get; }
        ValidationSeverity Severity { get; }
        IColumns Columns { get; }
        bool IsValid(DataRow dataRow);
        string GetMessage(DataRow dataRow);
    }
}
