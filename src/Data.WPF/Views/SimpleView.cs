using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Views
{
    public class SimpleView : ContentControl, IBaseView
    {
        static SimpleView()
        {
            FocusableProperty.OverrideMetadata(typeof(SimpleView), new FrameworkPropertyMetadata(BooleanBoxes.False));
        }

        public SimplePresenter Presenter { get; private set; }

        BasePresenter IBaseView.Presenter
        {
            get { return Presenter; }
            set { Presenter = (SimplePresenter)value; }
        }

        private LayoutManager LayoutManager
        {
            get { return Presenter.LayoutManager; }
        }

        void IBaseView.RefreshScalarValidation()
        {
            var layoutManager = LayoutManager;
            if (layoutManager != null)
                this.RefreshValidation(layoutManager.GetScalarValidationInfo());
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            System.Diagnostics.Debug.WriteLine(string.Format("GotFocus: {0}", e.OriginalSource.ToString()));
        }
    }
}
