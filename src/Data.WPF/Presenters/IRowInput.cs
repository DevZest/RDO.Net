using System.Windows;

namespace DevZest.Data.Presenters
{
    internal interface IRowInput
    {
        FlushErrorMessage GetFlushError(UIElement element);
        IValidationMessageGroup GetErrors(RowPresenter rowPresenter);
        IValidationMessageGroup GetWarnings(RowPresenter rowPresenter);
    }
}
