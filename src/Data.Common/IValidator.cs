namespace DevZest.Data
{
    /// <summary></summary>
    internal interface IValidator
    {
        IColumnValidationMessages Validate(DataRow dataRow);
    }
}
