using System.Collections.Generic;

namespace DevZest.Data.Presenters.Services
{
    public interface ISortService : IService
    {
        IReadOnlyList<ColumnSort> OrderBy { get; set; }
    }
}
