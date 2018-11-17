using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Data.Views
{
    public class ScalarBagView : ContentControl, IBaseView
    {
        static ScalarBagView()
        {
            FocusableProperty.OverrideMetadata(typeof(ScalarBagView), new FrameworkPropertyMetadata(BooleanBoxes.False));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(ScalarBagView), new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(ScalarBagView), new FrameworkPropertyMetadata(KeyboardNavigationMode.None));
        }

        public ScalarBagPresenter Presenter { get; private set; }

        BasePresenter IBaseView.Presenter
        {
            get { return Presenter; }
            set { Presenter = (ScalarBagPresenter)value; }
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
