namespace DevZest.Data
{
    /// <summary></summary>
    internal interface IValidator
    {
        ColumnValidationMessage Validate(DataRow dataRow);
    }
}
