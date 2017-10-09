using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Presenters.Services;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System;

namespace DevZest.Data.Views
{
    public class ForeignKeyBox : ButtonBase, IRowElement
    {
        public interface IEditService : IService
        {
            void Edit(RowPresenter rowPresenter, KeyBase foreignKey);
        }

        private static readonly DependencyPropertyKey CanResetPropertyKey = DependencyProperty.RegisterReadOnly(nameof(CanReset), typeof(bool),
            typeof(ForeignKeyBox), new FrameworkPropertyMetadata(BooleanBoxes.False));
        public static readonly DependencyProperty CanResetProperty = CanResetPropertyKey.DependencyProperty;

        public static readonly RoutedUICommand ResetCommand = new RoutedUICommand();

        static ForeignKeyBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ForeignKeyBox), new FrameworkPropertyMetadata(typeof(ForeignKeyBox)));
        }

        public ForeignKeyBox()
        {
            CommandBindings.Add(new CommandBinding(ResetCommand, ExecReset, CanExecReset));
        }

        private void CanExecReset(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanReset;
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

        public KeyBase ForeignKey { get; set; }

        private RowPresenter RowPresenter
        {
            get { return this.GetRowPresenter(); }
        }

        private DataPresenter DataPresenter
        {
            get { return RowPresenter?.DataPresenter; }
        }

        public bool CanReset
        {
            get { return (bool)GetValue(CanResetProperty); }
            private set { SetValue(CanResetPropertyKey, BooleanBoxes.Box(value)); }
        }

        private bool CalcCanReset(RowPresenter rowPresenter)
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
            var rowPresenter = RowPresenter;
            if (rowPresenter == null)
                return;

            var foreignKey = ForeignKey;
            if (foreignKey == null)
                return;

            var dataPresenter = DataPresenter;
            if (dataPresenter == null)
                return;

            var editService = dataPresenter.GetService<IEditService>();
            if (editService != null)
                editService.Edit(rowPresenter, foreignKey);
        }

        void IRowElement.Setup(RowPresenter rowPresenter)
        {
        }

        void IRowElement.Refresh(RowPresenter rowPresenter)
        {
            CanReset = CalcCanReset(rowPresenter);
        }

        void IRowElement.Cleanup(RowPresenter rowPresenter)
        {
        }
    }
}

