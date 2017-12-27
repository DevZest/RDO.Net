using System.Windows;

namespace DevZest.Data.Presenters
{
    internal interface IRowInput
    {
        FlushErrorMessage GetFlushError(UIElement element);
        IColumnValidationMessages GetValidationErrors(RowPresenter rowPresenter);
        IColumnValidationMessages GetValidationWarnings(RowPresenter rowPresenter);
        IColumns Target { get; }
    }
}
