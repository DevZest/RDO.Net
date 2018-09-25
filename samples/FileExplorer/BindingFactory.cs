using DevZest.Data.Presenters;
using System.IO;

namespace FileExplorer
{
    public static class BindingFactory
    {
        private static void Refresh(DirectoryTreeItemView v, DirectoryTreeItem _, RowPresenter p)
        {
            var depth = p.Depth;
            v.Refresh(depth, depth == 0 ? Icons.DiskDrive : Icons.Folder, GetDisplayText(_, p));
        }

        public static RowBinding<DirectoryTreeItemView> BindToDirectoryTreeItemView(this DirectoryTreeItem _)
        {
            return new RowBinding<DirectoryTreeItemView>((e, r) => Refresh(e, _, r));
        }

        private static string GetDisplayText(DirectoryTreeItem _, RowPresenter p)
        {
            var result = p.GetValue(_.Path);
            var depth = p.Depth;
            if (depth > 0)
                result = Path.GetFileName(result);
            return result;
        }

        private static void Refresh(LargeIconListItemView v, LargeIconListItem _, RowPresenter p)
        {
            v.ImageSource = p.GetValue(_.LargeIcon);
        }

        public static RowCompositeBinding<LargeIconListItemView> BindToLargeIconListItemView(this LargeIconListItem _)
        {
            var textBoxBinding = _.DisplayName.BindToTextBox();
            var textBlockBinding = _.DisplayName.BindToTextBlock();

            return new RowCompositeBinding<LargeIconListItemView>((e, r) => Refresh(e, _, r))
                .AddChild(textBoxBinding.MergeIntoInPlaceEditor(textBlockBinding), v => v.InPlaceEditor);
        }
    }
}
