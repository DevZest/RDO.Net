using System.Collections.Generic;
using System.Windows.Input;

namespace DevZest.Data.Presenters.Services
{
    public class DataViewCommands : IService
    {
        public static readonly RoutedUICommand RetryDataLoad = new RoutedUICommand(UIText.DataViewCommands_RetryDataLoadCommandText, nameof(RetryDataLoad), typeof(DataViewCommands));
        public static readonly RoutedUICommand CancelDataLoad = new RoutedUICommand(UIText.DataViewCommands_CancelDataLoadCommandText, nameof(CancelDataLoad), typeof(DataViewCommands));

        public DataPresenter DataPresenter { get; set; }

        protected internal virtual IEnumerable<CommandEntry> CommandEntries
        {
            get
            {
                yield return CancelDataLoad.CommandBinding(CancelLoadData, CanCancelLoadData);
                yield return RetryDataLoad.CommandBinding(ReloadData, CanReloadData);
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
    }
}
