using DevZest.Data;
using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    public interface IValidationDictionary : IReadOnlyDictionary<RowPresenter, IColumnValidationMessages>
    {
        bool IsSealed { get; }
        IValidationDictionary Seal();
        IValidationDictionary Add(RowPresenter rowPresenter, IColumnValidationMessages validationMessages);
        IValidationDictionary Remove(RowPresenter rowPresenter);
    }
}
