namespace DevZest.Data
{
    /// <summary></summary>
    internal interface IValidator
    {
        DataValidationError Validate(DataRow dataRow);
    }
}
