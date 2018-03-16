using DevZest.Data.Presenters;
using System.Collections.Generic;
using System.Windows.Input;

namespace DevZest.Data.Views
{
    partial class GridCell
    {
        public static class Commands
        {
            public static readonly RoutedUICommand ToggleMode = new RoutedUICommand();
            public static readonly RoutedUICommand ExitEditMode = new RoutedUICommand();
            public static readonly RoutedUICommand ActivateGridCell = new RoutedUICommand();
            public static readonly RoutedUICommand SelectTo = new RoutedUICommand();
            public static RoutedCommand SelectAll { get { return ApplicationCommands.SelectAll; } }
        }

        public interface ICommandService : IService
        {
            IEnumerable<CommandEntry> GetCommandEntries(GridCell gridCell);
        }

        private sealed class CommandService : ICommandService
        {
            public DataPresenter DataPresenter { get; private set; }

            private Presenter Presenter
            {
                get { return DataPresenter.GetService<Presenter>(); }
            }

            private GridCellMode Mode
            {
                get { return Presenter.Mode; }
            }

            public void Initialize(DataPresenter dataPresenter)
            {
                DataPresenter = dataPresenter;
            }

            public IEnumerable<CommandEntry> GetCommandEntries(GridCell gridCell)
            {
                yield return Commands.ToggleMode.Bind(ExecToggleMode, CanToggleMode, new KeyGesture(Key.F8));
                yield return Commands.ActivateGridCell.Bind(ExecActivateGridCell, CanActivateGridCell, new MouseGesture(MouseAction.LeftClick));
                yield return Commands.ExitEditMode.Bind(ExecToggleMode, CanExitEditMode, new KeyGesture(Key.Escape));
                yield return Commands.SelectTo.Bind(ExecSelectTo, CanSelectTo, new MouseGesture(MouseAction.LeftClick, ModifierKeys.Shift));
                yield return Commands.SelectAll.Bind(ExecSelectAll, CanSelectTo);
            }

            private void CanToggleMode(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = CanToggleMode((GridCell)sender);
                if (!e.CanExecute)
                    e.ContinueRouting = true;
            }

            private bool CanToggleMode(GridCell gridCell)
            {
                return Presenter.CanToggleMode(gridCell);
            }

            private void ExecToggleMode(object sender, ExecutedRoutedEventArgs e)
            {
                e.Handled = ToggleMode((GridCell)sender);
            }

            private bool ToggleMode(GridCell gridCell)
            {
                Presenter.ToggleMode(gridCell);
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
                return gridCell.IsCurrent && Mode == GridCellMode.Select && gridCell.IsEditable;
            }

            private void CanExitEditMode(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = CanExitEditMode((GridCell)sender);
                if (!e.CanExecute)
                    e.ContinueRouting = true;
            }

            private bool CanExitEditMode(GridCell gridCell)
            {
                return gridCell.IsCurrent && Mode == GridCellMode.Edit;
            }

            private void CanActivateGridCell(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = CanActivate((GridCell)sender);
                if (!e.CanExecute)
                    e.ContinueRouting = true;
            }

            private bool CanActivate(GridCell gridCell)
            {
                return Presenter.PredictActivate(gridCell).HasValue;
            }

            private void ExecActivateGridCell(object sender, ExecutedRoutedEventArgs e)
            {
                e.Handled = Activate((GridCell)sender);
            }

            private bool Activate(GridCell gridCell)
            {
                Presenter.Activate(gridCell);
                return true;
            }

            private void CanSelectTo(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = CanSelectTo((GridCell)sender);
                if (!e.CanExecute)
                    e.ContinueRouting = true;
            }

            private bool CanSelectTo(GridCell gridCell)
            {
                return Mode == GridCellMode.Select;
            }

            private void ExecSelectTo(object sender, ExecutedRoutedEventArgs e)
            {
                e.Handled = SelectTo((GridCell)sender);
            }

            private bool SelectTo(GridCell gridCell)
            {
                Presenter.Select(gridCell, true);
                return true;
            }

            private void ExecSelectAll(object sender, ExecutedRoutedEventArgs e)
            {
                e.Handled = SelectAll();
            }

            private bool SelectAll()
            {
                Presenter.SelectAll();
                return true;
            }
        }
    }
}
