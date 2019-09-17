using DevZest.Data.Presenters.Primitives;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using DevZest.Data.Presenters;
using System.Windows.Controls;

namespace DevZest.Data.Views
{
    /// <summary>
    /// Represents a button that can select/deselect all rows.
    /// </summary>
    /// <remarks>Use this control together with <see cref="RowHeader"/>. Alternatively you can use a <see cref="CheckBox"/> via data binding.</remarks>
    public class GridHeader : ToggleButton, IScalarElement, RowSelectionWiper.ISelector
    {
        /// <summary>
        /// Identifies <see cref="SeparatorBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SeparatorBrushProperty = DependencyProperty.Register(nameof(SeparatorBrush), typeof(Brush),
            typeof(GridHeader), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Idenfifies <see cref="SeparatorVisibility"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SeparatorVisibilityProperty = DependencyProperty.Register(nameof(SeparatorVisibility), typeof(Visibility),
            typeof(GridHeader), new FrameworkPropertyMetadata(Visibility.Visible));

        static GridHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridHeader), new FrameworkPropertyMetadata(typeof(GridHeader)));
        }

        /// <summary>
        /// Gets or sets the brush that draws the separation between headers.
        /// </summary>
        public Brush SeparatorBrush
        {
            get { return (Brush)GetValue(SeparatorBrushProperty); }
            set { SetValue(SeparatorBrushProperty, value); }
        }

        /// <summary>
        /// Gets or sets the user interface (UI) visibility of the row header separator lines.
        /// </summary>
        public Visibility SeparatorVisibility
        {
            get { return (Visibility)GetValue(SeparatorVisibilityProperty); }
            set { SetValue(SeparatorVisibilityProperty, value); }
        }

        void IScalarElement.Setup(ScalarPresenter scalarPresenter)
        {
            var dataPresenter = scalarPresenter.DataPresenter;
            RowSelectionWiper.EnsureSetup(dataPresenter);
        }

        void IScalarElement.Refresh(ScalarPresenter scalarPresenter)
        {
        }

        void IScalarElement.Cleanup(ScalarPresenter scalarPresenter)
        {
        }
    }
}
