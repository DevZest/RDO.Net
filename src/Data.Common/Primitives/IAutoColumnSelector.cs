using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    internal interface IAutoColumnSelector
    {
        Column Select(Column column);
        IAutoColumnSelector Merge(IEnumerable<Column> columns);
    }
}
