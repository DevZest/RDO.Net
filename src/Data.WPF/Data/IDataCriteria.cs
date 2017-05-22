using DevZest.Data;
using System;

namespace DevZest.Windows.Data
{
    public interface IDataCriteria
    {
        Column<bool?> GetWhere(int depth);
        ColumnSort[] GetOrderBy(int depth);
        void Apply(Func<Model, Column<bool?>> where, Func<Model, ColumnSort[]> orderBy);
    }
}
