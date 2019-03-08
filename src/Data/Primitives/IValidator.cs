using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.Primitives
{
    /// <summary></summary>
    public interface IValidator
    {
        IValidatorAttribute Attribute { get; }
        Model Model { get; }
        IColumns SourceColumns { get; }
        DataValidationError Validate(DataRow dataRow);
    }
}
