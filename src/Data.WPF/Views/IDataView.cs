using DevZest.Data.Presenters.Primitives;

namespace DevZest.Data.Views
{
    internal interface IDataView
    {
        CommonPresenter Presenter { get; set; }
        void RefreshScalarValidation();
    }
}
