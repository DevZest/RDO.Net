using System.Windows;

namespace DevZest.Data.Presenters
{
    internal interface IRowInput
    {
        FlushErrorMessage GetFlushError(UIElement element);
        IColumnValidationMessages GetErrors(RowPresenter rowPresenter);
        IColumnValidationMessages GetWarnings(RowPresenter rowPresenter);
    }
}
