using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System.Collections.Generic;
using System.Windows.Input;

namespace DevZest.Data.Views
{
    partial class DataView
    {
        public static class Commands
        {
            public static readonly RoutedUICommand RetryDataLoad = new RoutedUICommand(UserMessages.DataViewCommands_RetryDataLoadCommandText, nameof(RetryDataLoad), typeof(Commands));
            public static readonly RoutedUICommand CancelDataLoad = new RoutedUICommand(UserMessages.DataViewCommands_CancelDataLoadCommandText, nameof(CancelDataLoad), typeof(Commands));
            public static readonly RoutedUICommand ToggleEditScalars = new RoutedUICommand();
            public static readonly RoutedUICommand BeginEditScalars = new RoutedUICommand();
            public static readonly RoutedUICommand CancelEditScalars = new RoutedUICommand();
            public static readonly RoutedUICommand EndEditScalars = new RoutedUICommand();
            public static RoutedUICommand ScrollUp { get { return ComponentCommands.MoveFocusUp; } }
            public static RoutedUICommand ScrollDown { get { return ComponentCommands.MoveFocusDown; } }
            public static RoutedUICommand ScrollLeft { get { return ComponentCommands.MoveFocusBack; } }
            public static RoutedUICommand ScrollRight { get { return ComponentCommands.MoveFocusForward; } }
            public static RoutedUICommand ScrollPageUp { get { return ComponentCommands.MoveFocusPageUp; } }
            public static RoutedUICommand ScrollPageDown { get { return ComponentCommands.MoveFocusPageDown; } }
        }

        public interface ICommandService : IService
        {
            IEnumerable<CommandEntry> GetCommandEntries(DataView dataView);
        }

        private sealed class CommandService : ICommandService
        {
            public DataPresenter DataPresenter { get; private set; }

            public void Initialize(DataPresenter dataPresenter)
            {
                DataPresenter = dataPresenter;
            }

            public IEnumerable<CommandEntry> GetCommandEntries(DataView dataView)
            {
                yield return Commands.CancelDataLoad.Bind(CancelLoadData, CanCancelLoadData);
                yield return Commands.RetryDataLoad.Bind(ReloadData, CanReloadData);
                yield return Commands.ToggleEditScalars.Bind(ToggleEditScalars);
                yield return Commands.BeginEditScalars.Bind(BeginEditScalars, CanBeginEditScalars);
                yield return Commands.CancelEditScalars.Bind(CancelEditScalars, CanCancelEditScalars);
                yield return Commands.EndEditScalars.Bind(EndEditScalars, CanCancelEditScalars);
                if (DataPresenter.LayoutOrientation.HasValue)
                {
                    yield return Commands.ScrollUp.Bind(ExecScrollUp);
                    yield return Commands.ScrollDown.Bind(ExecScrollDown);
                    yield return Commands.ScrollLeft.Bind(ExecScrollLeft);
                    yield return Commands.ScrollRight.Bind(ExecScrollRight);
                    yield return Commands.ScrollPageUp.Bind(ExecScrollPageUp);
                    yield return Commands.ScrollPageDown.Bind(ExecScrollPageDown);
                }
            }

            private void ReloadData(object sender, ExecutedRoutedEventArgs e)
            {
                DataPresenter.Reload();
            }

            private void CanReloadData(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = DataPresenter.CanReload;
            }

            private void CancelLoadData(object sender, ExecutedRoutedEventArgs e)
            {
                DataPresenter.CancelLoading();
            }

            private void CanCancelLoadData(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = DataPresenter.CanCancelLoading;
            }

            private void ToggleEditScalars(object sender, ExecutedRoutedEventArgs e)
            {
                var scalarContainer = ((DataView)sender).DataPresenter.ScalarContainer;
                if (scalarContainer.IsEditing)
                    scalarContainer.EndEdit();
                else
                    scalarContainer.BeginEdit();
            }

            private void CanBeginEditScalars(object sender, CanExecuteRoutedEventArgs e)
            {
                var scalarContainer = ((DataView)sender).DataPresenter.ScalarContainer;
                e.CanExecute = !scalarContainer.IsEditing;
                if (!e.CanExecute)
                    e.ContinueRouting = true;
            }

            private void BeginEditScalars(object sender, ExecutedRoutedEventArgs e)
            {
                var scalarContainer = ((DataView)sender).DataPresenter.ScalarContainer;
                scalarContainer.BeginEdit();
            }

            private void CanCancelEditScalars(object sender, CanExecuteRoutedEventArgs e)
            {
                var scalarContainer = ((DataView)sender).DataPresenter.ScalarContainer;
                e.CanExecute = scalarContainer.IsEditing;
                if (!e.CanExecute)
                    e.ContinueRouting = true;
            }

            private void CancelEditScalars(object sender, ExecutedRoutedEventArgs e)
            {
                var scalarContainer = ((DataView)sender).DataPresenter.ScalarContainer;
                scalarContainer.CancelEdit();
            }

            private void EndEditScalars(object sender, ExecutedRoutedEventArgs e)
            {
                var scalarContainer = ((DataView)sender).DataPresenter.ScalarContainer;
                scalarContainer.EndEdit();
            }

            private IScrollable Scrollable
            {
                get { return DataPresenter.Scrollable; }
            }

            private DataView View
            {
                get { return DataPresenter.View; }
            }

            private void ExecScrollUp(object sender, ExecutedRoutedEventArgs e)
            {
                Scrollable.ScrollBy(0, -View.ScrollLineHeight);
            }

            private void ExecScrollDown(object sender, ExecutedRoutedEventArgs e)
            {
                Scrollable.ScrollBy(0, View.ScrollLineHeight);
            }

            private void ExecScrollLeft(object sender, ExecutedRoutedEventArgs e)
            {
                Scrollable.ScrollBy(-View.ScrollLineWidth, 0);
            }

            private void ExecScrollRight(object sender, ExecutedRoutedEventArgs e)
            {
                Scrollable.ScrollBy(View.ScrollLineWidth, 0);
            }

            private void ExecScrollPageUp(object sender, ExecutedRoutedEventArgs e)
            {
                Scrollable.ScrollPageUp();
            }

            private void ExecScrollPageDown(object sender, ExecutedRoutedEventArgs e)
            {
                Scrollable.ScrollPageDown();
            }
        }
    }
}
