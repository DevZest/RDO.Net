using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System.Collections.Generic;
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
            bool CanLookup(PrimaryKey foreignKey);
            void BeginLookup(ForeignKeyBox foreignKeyBox);
        }

        private static readonly DependencyPropertyKey ValueBagPropertyKey = DependencyProperty.RegisterReadOnly(nameof(ValueBag),
            typeof(ColumnValueBag), typeof(ForeignKeyBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ValueBagProperty = ValueBagPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey CanClearValuePropertyKey = DependencyProperty.RegisterReadOnly(nameof(CanClearValue), typeof(bool),
            typeof(ForeignKeyBox), new FrameworkPropertyMetadata(BooleanBoxes.False));
        public static readonly DependencyProperty CanClearValueProperty = CanClearValuePropertyKey.DependencyProperty;

        public abstract class Commands
        {
            internal static readonly RoutedUICommand Lookup = new RoutedUICommand();
            public static readonly RoutedUICommand ClearValue = new RoutedUICommand();
        }

        public interface ICommandService : IService
        {
            IEnumerable<CommandEntry> GetCommandEntries(ForeignKeyBox foreignKeyBox);
        }

        private sealed class CommandService : ICommandService
        {
            public DataPresenter DataPresenter { get; private set; }

            public void Initialize(DataPresenter dataPresenter)
            {
                DataPresenter = dataPresenter;
            }

            public IEnumerable<CommandEntry> GetCommandEntries(ForeignKeyBox foreignKeyBox)
            {
                yield return Commands.Lookup.Bind(ExecLookup, CanExecLookup);
                yield return Commands.ClearValue.Bind(ExecClearValue, CanExecClearValue, new KeyGesture(Key.Delete));
            }

            private void ExecLookup(object sender, ExecutedRoutedEventArgs e)
            {
                ((ForeignKeyBox)sender).ExecLookup();
            }

            private void CanExecLookup(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = ((ForeignKeyBox)sender).CanExecLookup;
            }

            private void ExecClearValue(object sender, ExecutedRoutedEventArgs e)
            {
                ((ForeignKeyBox)sender).ClearValue();
            }

            private void CanExecClearValue(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = ((ForeignKeyBox)sender).CanClearValue;
                if (!e.CanExecute)
                    e.ContinueRouting = true;
            }
        }

        static ForeignKeyBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ForeignKeyBox), new FrameworkPropertyMetadata(typeof(ForeignKeyBox)));
            ServiceManager.Register<ICommandService, CommandService>();
        }

        public ForeignKeyBox()
        {
            ValueBag = new ColumnValueBag();
        }

        private bool CanExecLookup
        {
            get
            {
                var lookupService = DataPresenter?.GetService<ILookupService>();
                return lookupService == null ? false : lookupService.CanLookup(ForeignKey);
            }
        }

        private void ExecLookup()
        {
            var lookupService = DataPresenter?.GetService<ILookupService>();
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

        public void ClearValue()
        {
            var valueBag = ValueBag.Clone();
            valueBag.ResetValues();
            EndLookup(valueBag);
        }

        public PrimaryKey ForeignKey { get; internal set; }

        public ModelExtender Extender { get; internal set; }

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

        protected virtual ICommandService GetCommandService(DataPresenter dataPresenter)
        {
            return dataPresenter.GetService<ICommandService>();
        }

        void IRowElement.Setup(RowPresenter rowPresenter)
        {
            this.SetupCommandEntries(GetCommandService(rowPresenter.DataPresenter), GetCommandEntries);
            Command = Commands.Lookup;  // Command needs to be set after command bindings otherwise IsEnabled will be false
        }

        private static IEnumerable<CommandEntry> GetCommandEntries(ICommandService commandService, ForeignKeyBox foreignKeyBox)
        {
            return commandService.GetCommandEntries(foreignKeyBox);
        }

        void IRowElement.Refresh(RowPresenter rowPresenter)
        {
            CanClearValue = CalcCanClearValue(rowPresenter);
        }

        void IRowElement.Cleanup(RowPresenter rowPresenter)
        {
            Command = null;
            ForeignKey = null;
            Extender = null;
            ValueBag.Clear();
            this.CleanupCommandEntries();
        }
    }
}

