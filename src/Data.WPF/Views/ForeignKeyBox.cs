using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DevZest.Data.Views
{
    /// <summary>
    /// Represents a clickable box to display and edit foreign key data
    /// </summary>
    public class ForeignKeyBox : ButtonBase, IRowElement
    {
        /// <summary>
        /// Service to perform foreign key data lookup operation.
        /// </summary>
        public interface ILookupService : IService
        {
            /// <summary>
            /// Determines whether lookup operation can be performed for specified foreign key.
            /// </summary>
            /// <param name="foreignKey">The foreign key.</param>
            /// <returns><see langword="true"/> if lookup operation can be performed for specified foreign key, otherwise <see langword="false"/>.</returns>
            bool CanLookup(CandidateKey foreignKey);

            /// <summary>
            /// Begins the lookup operation.
            /// </summary>
            /// <param name="foreignKeyBox">The <see cref="ForeignKeyBox"/>.</param>
            void BeginLookup(ForeignKeyBox foreignKeyBox);
        }

        private static readonly DependencyPropertyKey ValueBagPropertyKey = DependencyProperty.RegisterReadOnly(nameof(ValueBag),
            typeof(ColumnValueBag), typeof(ForeignKeyBox), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Identifies <see cref="ValueBag"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueBagProperty = ValueBagPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey CanClearValuePropertyKey = DependencyProperty.RegisterReadOnly(nameof(CanClearValue), typeof(bool),
            typeof(ForeignKeyBox), new FrameworkPropertyMetadata(BooleanBoxes.False));

        /// <summary>
        /// Identifies <see cref="CanClearValue"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CanClearValueProperty = CanClearValuePropertyKey.DependencyProperty;

        /// <summary>
        /// Contains commands implemented by <see cref="ForeignKeyBox"/> class.
        /// </summary>
        public abstract class Commands
        {
            internal static readonly RoutedUICommand Lookup = new RoutedUICommand();

            /// <summary>
            /// Clears underlying foreign key data.
            /// </summary>
            public static readonly RoutedUICommand ClearValue = new RoutedUICommand();
        }

        /// <summary>
        /// Customizable service to provide command implementations.
        /// </summary>
        public interface ICommandService : IService
        {
            /// <summary>
            /// Retrieves command implementations for specified <see cref="ForeignKeyBox"/>.
            /// </summary>
            /// <param name="foreignKeyBox">The specified <see cref="ForeignKeyBox"/>.</param>
            /// <returns>The retrieved command implementations.</returns>
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
            Service.Register<ICommandService, CommandService>();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ForeignKeyBox"/> class.
        /// </summary>
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

        /// <summary>
        /// Ends the lookup operation and updates this <see cref="ForeignKeyBox"/>.
        /// </summary>
        /// <param name="valueBag">The <see cref="ColumnValueBag"/> that contains foreign key and lookup data.</param>
        public void EndLookup(ColumnValueBag valueBag)
        {
            if (valueBag != null)
            {
                ValueBag = valueBag;
                DataPresenter?.InvalidateView();
            }
        }

        /// <summary>
        /// Clears the underlying foreign key and lookup data.
        /// </summary>
        public void ClearValue()
        {
            var valueBag = ValueBag.Clone();
            valueBag.ResetValues();
            EndLookup(valueBag);
        }

        /// <summary>
        /// Gets the foreign key column(s).
        /// </summary>
        public CandidateKey ForeignKey { get; internal set; }

        /// <summary>
        /// Gets the lookup column(s).
        /// </summary>
        public Projection Lookup { get; internal set; }

        /// <summary>
        /// Gets the <see cref="ColumnValueBag"/> which contains the underlying foreign key and lookup data.
        /// </summary>
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

        /// <summary>
        /// Gets a value indicates whether underlying foreign key data can be cleared. This is a dependency property.
        /// </summary>
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

        private ICommandService GetCommandService(DataPresenter dataPresenter)
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
            Lookup = null;
            ValueBag.Clear();
            this.CleanupCommandEntries();
        }
    }
}

