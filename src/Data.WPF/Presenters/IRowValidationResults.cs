using DevZest.Data;
using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    public interface IRowValidationResults : IReadOnlyDictionary<RowPresenter, IDataValidationErrors>
    {
        bool IsSealed { get; }
        IRowValidationResults Seal();
        IRowValidationResults Add(RowPresenter rowPresenter, IDataValidationErrors errors);
        IRowValidationResults Remove(RowPresenter rowPresenter);
    }
}
