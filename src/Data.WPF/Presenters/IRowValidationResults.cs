using DevZest.Data;
using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    public interface IRowValidationResults : IReadOnlyDictionary<RowPresenter, IColumnValidationMessages>
    {
        bool IsSealed { get; }
        IRowValidationResults Seal();
        IRowValidationResults Add(RowPresenter rowPresenter, IColumnValidationMessages validationMessages);
        IRowValidationResults Remove(RowPresenter rowPresenter);
    }
}
