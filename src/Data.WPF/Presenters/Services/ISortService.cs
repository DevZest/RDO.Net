using System.Collections.Generic;

namespace DevZest.Data.Presenters.Services
{
    public interface ISortService : IService
    {
        void Apply(IReadOnlyList<IColumnComparer> orderBy);
        IReadOnlyList<ColumnSort> OrderBy { get; set; }
    }
}
