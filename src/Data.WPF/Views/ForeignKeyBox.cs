using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Presenters.Services;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DevZest.Data.Views
{
    public class ForeignKeyBox : ButtonBase, IRowElement
    {
        public interface IEditingService : IService
        {
            ColumnValueBag Edit(KeyBase foreignKey);
        }

        private static readonly DependencyPropertyKey ForeignKeyValueBagPropertyKey = DependencyProperty.RegisterReadOnly(nameof(ForeignKeyValueBag),
            typeof(ColumnValueBag), typeof(ForeignKeyBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ForeignKeyValueBagProperty = ForeignKeyValueBagPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey CanClearValuePropertyKey = DependencyProperty.RegisterReadOnly(nameof(CanClearValue), typeof(bool),
            typeof(ForeignKeyBox), new FrameworkPropertyMetadata(BooleanBoxes.False));
        public static readonly DependencyProperty CanClearValueProperty = CanClearValuePropertyKey.DependencyProperty;

        public static readonly RoutedUICommand ClearValueCommand = new RoutedUICommand();

        static ForeignKeyBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ForeignKeyBox), new FrameworkPropertyMetadata(typeof(ForeignKeyBox)));
        }

        public ForeignKeyBox()
        {
            CommandBindings.Add(new CommandBinding(ClearValueCommand, ExecReset, CanExecReset));
        }

        private void CanExecReset(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanClearValue;
        }

        private void ExecReset(object sender, ExecutedRoutedEventArgs e)
        {
            var rowPresenter = RowPresenter;
            for (int i = 0; i < ForeignKey.Count; i++)
            {
                var column = ForeignKey[i].Column;
                rowPresenter[column] = null;
            }
        }

        public KeyBase ForeignKey { get; internal set; }

        public ModelExtension ForeignKeyExtension { get; internal set; }

        public ColumnValueBag ForeignKeyValueBag
        {
            get { return (ColumnValueBag)GetValue(ForeignKeyValueBagProperty); }
            private set { SetValue(ForeignKeyValueBagPropertyKey, value); }
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

        protected override void OnClick()
        {
            base.OnClick();
            InvokeEdit();
        }

        private void InvokeEdit()
        {
            var foreignKey = ForeignKey;
            if (foreignKey == null)
                return;

            var editingService = DataPresenter?.GetService<IEditingService>();
            if (editingService == null)
                return;

            var valueBag = editingService.Edit(foreignKey);
            if (valueBag != null)
                ForeignKeyValueBag = valueBag;
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
            ForeignKeyExtension = null;
            ForeignKeyValueBag = null;
        }
    }
}

