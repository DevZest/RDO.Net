using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Views
{
    /// <summary>
    /// Represents a control that displays scalar data.
    /// </summary>
    public class SimpleView : ContentControl, IBaseView
    {
        static SimpleView()
        {
            FocusableProperty.OverrideMetadata(typeof(SimpleView), new FrameworkPropertyMetadata(BooleanBoxes.False));
        }

        /// <summary>
        /// Gets the presenter.
        /// </summary>
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

        /// <inheritdoc/>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            System.Diagnostics.Debug.WriteLine(string.Format("GotFocus: {0}", e.OriginalSource.ToString()));
        }
    }
}
