using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System.Windows.Controls;

namespace DevZest.Data.Views
{
    public class ScalarBagView : ContentControl, IDataView
    {

        public ScalarBagPresenter ScalarBagPresenter { get; private set; }

        DataPresenterBase IDataView.DataPresenter
        {
            get { return ScalarBagPresenter; }
            set { ScalarBagPresenter = (ScalarBagPresenter)value; }
        }

        private LayoutManager LayoutManager
        {
            get { return ScalarBagPresenter.LayoutManager; }
        }

        void IDataView.RefreshScalarValidation()
        {
            var layoutManager = LayoutManager;
            if (layoutManager != null)
                this.RefreshValidation(layoutManager.GetScalarValidationInfo());
        }
    }
}
