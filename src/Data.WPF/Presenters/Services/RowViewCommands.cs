using DevZest.Data.Views;
using System.Collections.Generic;
using System.Windows.Input;

namespace DevZest.Data.Presenters.Services
{
    public class RowViewCommands : IService
    {
        public static readonly RoutedUICommand Expand = new RoutedUICommand(UIText.RowViewCommands_ExpandCommandText, nameof(Expand), typeof(RowViewCommands));
        public static readonly RoutedUICommand Collapse = new RoutedUICommand(UIText.RowViewCommands_CollapseCommandText, nameof(Collapse), typeof(RowViewCommands));

        public DataPresenter DataPresenter { get; set; }

        protected internal virtual IEnumerable<CommandEntry> CommandEntries
        {
            get
            {
                yield return Expand.InputBinding(ToggleExpandState, CanExpand, new KeyGesture(Key.OemPlus));
                yield return Collapse.InputBinding(ToggleExpandState, CanCollapse, new KeyGesture(Key.OemMinus));
            }
        }

        private void ToggleExpandState(object sender, ExecutedRoutedEventArgs e)
        {
            var rowView = (RowView)sender;
            rowView.RowPresenter.ToggleExpandState();
        }

        private void CanExpand(object sender, CanExecuteRoutedEventArgs e)
        {
            var rowView = (RowView)sender;
            var rowPresenter = rowView.RowPresenter;
            e.CanExecute = rowPresenter.HasChildren && !rowPresenter.IsExpanded;
        }

        private void CanCollapse(object sender, CanExecuteRoutedEventArgs e)
        {
            var rowView = (RowView)sender;
            var rowPresenter = rowView.RowPresenter;
            e.CanExecute = rowPresenter.HasChildren && rowPresenter.IsExpanded;
        }
    }
}
