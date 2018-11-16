using DevZest.Data.Presenters.Primitives;

namespace DevZest.Data.Views
{
    internal interface ICommonView
    {
        CommonPresenter Presenter { get; set; }
        void RefreshScalarValidation();
    }
}
