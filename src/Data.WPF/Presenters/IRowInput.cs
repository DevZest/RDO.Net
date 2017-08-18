using System.Windows;

namespace DevZest.Data.Presenters
{
    internal interface IRowInput
    {
        ViewInputError GetInputError(UIElement element);
        IAbstractValidationMessageGroup GetErrors(RowPresenter rowPresenter);
        IValidationMessageGroup GetWarnings(RowPresenter rowPresenter);
    }
}
