using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    internal interface IAutoColumnSelector : IEnumerable<Column>
    {
        Column Select(Column column);
        IAutoColumnSelector Merge(IAutoColumnSelector selector);
    }
}
