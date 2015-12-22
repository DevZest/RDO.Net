using System.Collections.Generic;

namespace DevZest.Data
{
    public interface IColumnSet : IReadOnlyList<Column>
    {
        bool Contains(Column column);
    }
}
