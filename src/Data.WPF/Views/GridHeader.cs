using DevZest.Data.Presenters.Primitives;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using DevZest.Data.Presenters;

namespace DevZest.Data.Views
{
    public class GridHeader : ToggleButton, IScalarElement, IRowHeader
    {
        public abstract class Commands : RowHeader.Commands
        {
        }

        public static readonly DependencyProperty SeparatorBrushProperty = DependencyProperty.Register(nameof(SeparatorBrush), typeof(Brush),
            typeof(GridHeader), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SeparatorVisibilityProperty = DependencyProperty.Register(nameof(SeparatorVisibility), typeof(Visibility),
            typeof(GridHeader), new FrameworkPropertyMetadata(Visibility.Visible));

        static GridHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridHeader), new FrameworkPropertyMetadata(typeof(GridHeader)));
        }

        public Brush SeparatorBrush
        {
            get { return (Brush)GetValue(SeparatorBrushProperty); }
            set { SetValue(SeparatorBrushProperty, value); }
        }

        public Visibility SeparatorVisibility
        {
            get { return (Visibility)GetValue(SeparatorVisibilityProperty); }
            set { SetValue(SeparatorVisibilityProperty, value); }
        }

        void IScalarElement.Setup(ScalarPresenter scalarPresenter)
        {
            var dataPresenter = scalarPresenter.DataPresenter;
            RowHeader.EnsureFocusTrackerInitialized(dataPresenter);
            this.SetupCommandEntries(dataPresenter.GetService<RowHeader.ICommandService>().GetCommandEntries(null));
        }

        void IScalarElement.Refresh(ScalarPresenter scalarPresenter)
        {
        }

        void IScalarElement.Cleanup(ScalarPresenter scalarPresenter)
        {
            this.CleanupCommandEntries();
            this.CleanupCommandEntries();
        }
    }
}
