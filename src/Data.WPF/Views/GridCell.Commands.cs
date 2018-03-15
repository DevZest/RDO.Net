using DevZest.Data.Presenters;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace DevZest.Data.Views
{
    partial class GridCell
    {
        public static class Commands
        {
            public static readonly RoutedUICommand ToggleMode = new RoutedUICommand();
            public static readonly RoutedUICommand EnterEditMode = new RoutedUICommand();
            public static readonly RoutedUICommand ExitEditMode = new RoutedUICommand();
        }

        public interface ICommandService : IService
        {
            IEnumerable<CommandEntry> GetCommandEntries(GridCell gridCell);
        }

        private sealed class CommandService : ICommandService
        {
            public DataPresenter DataPresenter { get; private set; }

            private GridCellMode Mode
            {
                get { return DataPresenter.GetService<Presenter>().Mode; }
            }

            public void Initialize(DataPresenter dataPresenter)
            {
                DataPresenter = dataPresenter;
            }

            public IEnumerable<CommandEntry> GetCommandEntries(GridCell gridCell)
            {
                yield return Commands.ToggleMode.Bind(ExecToggleMode, CanToggleMode, new KeyGesture(Key.F8));
                yield return Commands.EnterEditMode.Bind(ExecToggleMode, CanEnterEditMode, new MouseGesture(MouseAction.LeftClick), new KeyGesture(Key.Enter));
                yield return Commands.ExitEditMode.Bind(ExecToggleMode, CanExitEditMode, new KeyGesture(Key.Escape));
            }

            private void CanToggleMode(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = CanToggleMode((GridCell)sender);
                if (!e.CanExecute)
                    e.ContinueRouting = true;
            }

            private bool CanToggleMode(GridCell gridCell)
            {
                return Mode == GridCellMode.Edit ? true : IsEditable(gridCell);
            }

            private static bool IsEditable(GridCell gridCell)
            {
                var rowBinding = gridCell.GetBinding() as RowBinding;
                return rowBinding != null && rowBinding.IsEditable;
            }

            private void ExecToggleMode(object sender, ExecutedRoutedEventArgs e)
            {
                e.Handled = ToggleMode((GridCell)sender);
            }

            private bool ToggleMode(GridCell gridCell)
            {
                bool isKeyboardFocusWithin = gridCell.IsKeyboardFocusWithin;
                var presenter = DataPresenter.GetService<Presenter>();
                var mode = presenter.Mode;
                presenter.Mode = mode == GridCellMode.Edit ? GridCellMode.Select : GridCellMode.Edit;
                gridCell.Refresh();
                if (isKeyboardFocusWithin && !gridCell.ContainsKeyboardFocus())
                    gridCell.Focus();
                return true;
            }

            private void CanEnterEditMode(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = CanEnterEditMode((GridCell)sender);
                if (!e.CanExecute)
                    e.ContinueRouting = true;
            }

            private bool CanEnterEditMode(GridCell gridCell)
            {
                return gridCell.IsCurrent && Mode == GridCellMode.Select && IsEditable(gridCell);
            }

            private void CanExitEditMode(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = CanExitEditMode((GridCell)sender);
                if (!e.CanExecute)
                    e.ContinueRouting = true;
            }

            private bool CanExitEditMode(GridCell gridCell)
            {
                return !DataPresenter.IsEditing && gridCell.IsCurrent && Mode == GridCellMode.Edit;
            }
        }
    }
}
