using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System.Collections.Generic;
using System.Windows.Input;

namespace DevZest.Data.Tools
{
    partial class ModelVisualizer
    {
        private sealed class Presenter : TreePresenter<ModelMapper.TreeItem>, RowView.ICommandService
        {
            public static RoutedUICommand GotoSourceCommand { get { return ApplicationCommands.Open; } }

            public Presenter(ModelMapper modelMapper, CodeVisualizer visualizerPane, CommandBinding gotoSourceCommandBinding, CommandBinding showContextMenuCommandBinding)
                : base(visualizerPane, gotoSourceCommandBinding, showContextMenuCommandBinding)
            {
                ModelMapper = modelMapper;
                RefreshView(ModelMapper.TreeItems);
            }

            public ModelMapper ModelMapper { get; private set; }

            protected override RowBinding<TreeItemView> BindToTreeItemView()
            {
                return _.BindToTreeItemView(GetNameFormatString);
            }

            private string GetNameFormatString(RowPresenter p)
            {
                var kind = p.GetValue(_.Node).Kind;
                return kind == ModelMapper.NodeKind.Model ? UserMessages.ModelPresenter_ModelNameFormat : null;
            }

            public Presenter Refresh()
            {
                ModelMapper = ModelMapper.Refresh(ModelMapper, Document, SelectionSpan);
                if (ModelMapper != null)
                {
                    RefreshView(ModelMapper.TreeItems);
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
