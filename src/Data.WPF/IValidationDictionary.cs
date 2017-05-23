using DevZest.Data;
using System.Collections.Generic;

namespace DevZest.Windows
{
    public interface IValidationDictionary : IReadOnlyDictionary<RowPresenter, IValidationMessageGroup>
    {
        bool IsSealed { get; }
        IValidationDictionary Seal();
        IValidationDictionary Add(RowPresenter rowPresenter, IValidationMessageGroup validationMessages);
        IValidationDictionary Remove(RowPresenter rowPresenter);
    }
}
