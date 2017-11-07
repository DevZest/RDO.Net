using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace DevZest.Data.Views
{
    public class ForeignKeyBox : ButtonBase, IRowElement
    {
        public interface ILookupService : IService
        {
            bool CanLookup(KeyBase foreignKey);
            void BeginLookup(ForeignKeyBox foreignKeyBox);
        }

        private static readonly DependencyPropertyKey ValueBagPropertyKey = DependencyProperty.RegisterReadOnly(nameof(ValueBag),
            typeof(ColumnValueBag), typeof(ForeignKeyBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ValueBagProperty = ValueBagPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey CanClearValuePropertyKey = DependencyProperty.RegisterReadOnly(nameof(CanClearValue), typeof(bool),
            typeof(ForeignKeyBox), new FrameworkPropertyMetadata(BooleanBoxes.False));
        public static readonly DependencyProperty CanClearValueProperty = CanClearValuePropertyKey.DependencyProperty;

        public static class Commands
        {
            internal static readonly RoutedUICommand Lookup = new RoutedUICommand();
            public static readonly RoutedUICommand ClearValue = new RoutedUICommand();
        }

        static ForeignKeyBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ForeignKeyBox), new FrameworkPropertyMetadata(typeof(ForeignKeyBox)));
        }

        public ForeignKeyBox()
        {
            Command = Commands.Lookup;
            ValueBag = new ColumnValueBag();
            CommandBindings.Add(new CommandBinding(Commands.Lookup, ExecLookup, CanExecLookup));
            CommandBindings.Add(new CommandBinding(Commands.ClearValue, ExecClearValue, CanExecClearValue));
        }

        private void CanExecLookup(object sender, CanExecuteRoutedEventArgs e)
        {
            var lookupService = DataPresenter?.GetService<ILookupService>();
            e.CanExecute = lookupService == null ? false : lookupService.CanLookup(ForeignKey);
        }

        private void ExecLookup(object sender, ExecutedRoutedEventArgs e)
        {
            var lookupService = DataPresenter?.GetService<ILookupService>();
            if (lookupService != null)
                lookupService.BeginLookup(this);
        }

        public void EndLookup(ColumnValueBag valueBag)
        {
            if (valueBag != null)
            {
                ValueBag = valueBag;
                DataPresenter?.InvalidateView();
            }
        }

        private void CanExecClearValue(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanClearValue;
        }

        private void ExecClearValue(object sender, ExecutedRoutedEventArgs e)
        {
            ClearValue();
        }

        public void ClearValue()
        {
            var valueBag = ValueBag.Clone();
            valueBag.ResetValues();
            EndLookup(valueBag);
        }

        public KeyBase ForeignKey { get; internal set; }

        public ModelExtension Extension { get; internal set; }

        public ColumnValueBag ValueBag
        {
            get { return (ColumnValueBag)GetValue(ValueBagProperty); }
            private set { SetValue(ValueBagPropertyKey, value); }
        }

        private RowPresenter RowPresenter
        {
            get { return this.GetRowPresenter(); }
        }

        private DataPresenter DataPresenter
        {
            get { return RowPresenter?.DataPresenter; }
        }

        public bool CanClearValue
        {
            get { return (bool)GetValue(CanClearValueProperty); }
            private set { SetValue(CanClearValuePropertyKey, BooleanBoxes.Box(value)); }
        }

        private bool CalcCanClearValue(RowPresenter rowPresenter)
        {
            if (ForeignKey == null || ForeignKey.Count == 0)
                return false;

            for (int i = 0; i < ForeignKey.Count; i++)
            {
                var column = ForeignKey[i].Column;
                if (!column.IsNullable)
                    return false;
                if (rowPresenter.IsNull(column))
                    return false;
            }
            return true;
        }

        void IRowElement.Setup(RowPresenter rowPresenter)
        {
        }

        void IRowElement.Refresh(RowPresenter rowPresenter)
        {
            CanClearValue = CalcCanClearValue(rowPresenter);
        }

        void IRowElement.Cleanup(RowPresenter rowPresenter)
        {
            ForeignKey = null;
            Extension = null;
            ValueBag.Clear();
        }
    }
}

