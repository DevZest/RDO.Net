using System.Collections.Generic;

namespace DevZest.Data
{
    public interface IDataRowValidationResults : IReadOnlyList<DataRowValidationResult>
    {
        bool IsSealed { get; }

        IDataRowValidationResults Seal();

        IColumnValidationMessages this[DataRow dataRow] { get; }

        bool Contains(DataRow dataRow);

        bool TryGetValue(DataRow dataRow, out IColumnValidationMessages value);

        IDataRowValidationResults Add(DataRowValidationResult value);
    }
}
