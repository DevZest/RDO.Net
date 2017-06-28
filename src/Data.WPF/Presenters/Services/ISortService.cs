using System.Collections.Generic;

namespace DevZest.Data.Presenters.Services
{
    public interface ISortService : IService
    {
        IReadOnlyList<IColumnComparer> OrderBy { get; set; }
    }
}
