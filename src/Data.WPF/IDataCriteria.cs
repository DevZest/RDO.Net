using DevZest.Data;
using System;

namespace DevZest.Windows
{
    public interface IDataCriteria
    {
        Filter Where { get; }
        ColumnSort[] GetOrderBy(int depth);
        void Apply(Filter where, Func<Model, ColumnSort[]> orderBy);
    }
}
