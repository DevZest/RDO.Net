using DevZest.Data;

namespace DevZest.Windows.Data
{
    public interface IDataCriteria
    {
        _Boolean Where { get; }
        ColumnSort[] OrderBy { get; }
        void Apply(_Boolean where, ColumnSort[] orderBy);
    }
}
