using DevZest.Data.Presenters;
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
        }
    }
}
