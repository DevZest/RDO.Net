using DevZest.Data;
using System;

namespace DevZest.Windows
{
    public interface IDataCriteria
    {
        DataRowFilter Where { get; }
        ColumnSort[] GetOrderBy(int depth);
        void Apply(DataRowFilter where, Func<Model, ColumnSort[]> orderBy);
    }
}
