namespace DevZest.Data
{
    /// <summary></summary>
    public interface IValidator
    {
        IColumnValidationMessages Validate(DataRow dataRow);
    }
}
