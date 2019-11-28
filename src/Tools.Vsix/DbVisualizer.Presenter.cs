using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System.Collections.Generic;
using System.Windows.Input;

namespace DevZest.Data.Tools
{
    partial class DbVisualizer
    {
        private sealed class Presenter : TreePresenter<DbMapper.TreeItem>, RowView.ICommandService
        {
            public static RoutedUICommand GotoSourceCommand { get { return ApplicationCommands.Open; } }

            public Presenter(DbMapper dbMapper, CodeVisualizer visualizerPane, CommandBinding gotoSourceCommandBinding, CommandBinding showContextMenuCommandBinding)
                : base(visualizerPane, gotoSourceCommandBinding, showContextMenuCommandBinding)
            {
                DbMapper = dbMapper;
                RefreshView(DbMapper.TreeItems);
            }

            public DbMapper DbMapper { get; private set; }

            protected override RowBinding<TreeItemView> BindToTreeItemView()
            {
                return _.BindToTreeItemView();
            }

            public Presenter Refresh()
            {
                DbMapper = DbMapper.Refresh(DbMapper, Document, SelectionSpan);
                if (DbMapper != null)
                {
                    RefreshView(DbMapper.TreeItems);
                    return this;
                }
                else
                    return null;
            }

            IEnumerable<CommandEntry> RowView.ICommandService.GetCommandEntries(RowView rowView)
            {
                var baseService = ServiceManager.GetRegisteredService<RowView.ICommandService>(this);
                foreach (var entry in baseService.GetCommandEntries(rowView))
                    yield return entry;
                yield return GotoSourceCommand.Bind(new KeyGesture(Key.Enter), new MouseGesture(MouseAction.LeftDoubleClick));
            }
        }
    }
}
