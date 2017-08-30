using System.Collections.Generic;

namespace DevZest.Data
{
    public interface IValidationResult : IReadOnlyList<ValidationEntry>
    {
        bool IsSealed { get; }

        IValidationResult Seal();

        IColumnValidationMessages this[DataRow dataRow] { get; }

        bool Contains(DataRow dataRow);

        bool TryGetValue(DataRow dataRow, out IColumnValidationMessages value);

        IValidationResult Add(ValidationEntry value);
    }
}
