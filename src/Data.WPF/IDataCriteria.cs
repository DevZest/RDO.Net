using System.Collections.Generic;

namespace DevZest.Data.Windows
{
    public interface IDataCriteria
    {
        _Boolean Where { get; }
        ColumnSort[] OrderBy { get; }
        void Apply(_Boolean where, ColumnSort[] orderBy);
    }
}
