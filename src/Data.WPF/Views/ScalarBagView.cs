using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System.Windows.Controls;

namespace DevZest.Data.Views
{
    public class ScalarBagView : ContentControl, IDataView
    {

        public ScalarBagPresenter Presenter { get; private set; }

        CommonPresenter IDataView.Presenter
        {
            get { return Presenter; }
            set { Presenter = (ScalarBagPresenter)value; }
        }

        private LayoutManager LayoutManager
        {
            get { return Presenter.LayoutManager; }
        }

        void IDataView.RefreshScalarValidation()
        {
            var layoutManager = LayoutManager;
            if (layoutManager != null)
                this.RefreshValidation(layoutManager.GetScalarValidationInfo());
        }
    }
}
