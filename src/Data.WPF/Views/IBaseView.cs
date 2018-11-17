using DevZest.Data.Presenters;

namespace DevZest.Data.Views
{
    internal interface IBaseView
    {
        BasePresenter Presenter { get; set; }
        void RefreshScalarValidation();
    }
}
