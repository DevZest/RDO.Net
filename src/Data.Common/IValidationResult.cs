using System.Collections.Generic;

namespace DevZest.Data
{
    public interface IValidationResult : IReadOnlyList<ValidationEntry>
    {
        bool IsSealed { get; }

        IValidationResult Seal();

        IValidationMessageGroup this[DataRow dataRow] { get; }

        bool Contains(DataRow dataRow);

        bool TryGetValue(DataRow dataRow, out IValidationMessageGroup value);

        IValidationResult Add(ValidationEntry value);
    }
}
