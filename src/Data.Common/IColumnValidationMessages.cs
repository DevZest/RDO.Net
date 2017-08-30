using DevZest.Data.Primitives;
using System.Collections.Generic;

namespace DevZest.Data
{
    public interface IColumnValidationMessages : IReadOnlyList<ColumnValidationMessage>
    {
        IColumnValidationMessages Seal();
        IColumnValidationMessages Add(ColumnValidationMessage value);
    }
}
