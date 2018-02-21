using DevZest.Data.Presenters;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace FileExplorer
{
    public static class BindingFactory
    {
        private static void Refresh(FolderView v, Folder _, RowPresenter p)
        {
            var depth = p.Depth;
            v.Refresh(depth, depth == 0 ? Images.DiskDrive : Images.Folder, GetDisplayText(_, p));
        }

        public static RowBinding<FolderView> BindToFolderView(this Folder _)
        {
            return new RowBinding<FolderView>((e, r) => Refresh(e, _, r));
        }

        private static string GetDisplayText(Folder _, RowPresenter p)
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

        public static RowCompositeBinding<LargeIconListItemView> BindToLargeIconView(this LargeIconListItem _)
        {
            var textBoxBinding = _.DisplayName.BindToTextBox();
            var textBlockBinding = _.DisplayName.BindToTextBlock();

            return new RowCompositeBinding<LargeIconListItemView>((e, r) => Refresh(e, _, r))
                .AddChild(textBoxBinding.Input.AddToInPlaceEditor(textBlockBinding), v => v.InPlaceEditor);
        }
    }
}
