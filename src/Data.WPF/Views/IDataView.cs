using DevZest.Data.Presenters.Primitives;

namespace DevZest.Data.Views
{
    internal interface IDataView
    {
        DataPresenterBase DataPresenter { get; set; }
        void RefreshScalarValidation();
    }
}
