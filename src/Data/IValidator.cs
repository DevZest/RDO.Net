using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data
{
    /// <summary></summary>
    public interface IValidator
    {
        IValidatorAttribute Attribute { get; }
        Model Model { get; }
        DataValidationError Validate(DataRow dataRow);
    }
}
