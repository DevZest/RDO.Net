using System.Collections.Generic;

namespace DevZest.Data
{
    public interface IDataValidationResults : IReadOnlyList<DataValidationResult>
    {
        bool IsSealed { get; }

        IDataValidationResults Seal();

        IDataValidationErrors this[DataRow dataRow] { get; }

        bool Contains(DataRow dataRow);

        bool TryGetValue(DataRow dataRow, out IDataValidationErrors value);

        IDataValidationResults Add(DataValidationResult value);
    }
}
