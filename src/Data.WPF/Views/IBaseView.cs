using DevZest.Data.Presenters.Primitives;

namespace DevZest.Data.Views
{
    internal interface IBaseView
    {
        BasePresenter Presenter { get; set; }
        void RefreshScalarValidation();
    }
}
