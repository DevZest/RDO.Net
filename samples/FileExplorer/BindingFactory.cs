using DevZest.Data.Presenters;
using System.IO;
using System.Windows.Controls;

namespace FileExplorer
{
    public static class BindingFactory
    {
        private static void Refresh(FolderView element, Folder _, RowPresenter rowPresenter)
        {
            var depth = rowPresenter.Depth;
            element.Depth = depth;
            element.ImageSource = depth == 0 ? Images.DiskDrive : Images.Folder;
        }

        public static RowCompositeBinding<FolderView> AsFolderView(this Folder _)
        {
            var textBoxBinding = new RowBinding<TextBox>(onRefresh: (v, p) =>
            {
                v.Text = GetDisplayText(_, p);
            }).WithInput(TextBox.TextProperty, TextBox.LostFocusEvent, _.Path, (RowPresenter p, TextBox v) => GetPath(_, p, v.Text));

            var textBlockBinding = new RowBinding<TextBlock>(onRefresh: (v, p) =>
            {
                v.Text = GetDisplayText(_, p);
            });

            return new RowCompositeBinding<FolderView>((e, r) => Refresh(e, _, r))
                .AddChild(textBoxBinding.Input.AddToInPlaceEditor(textBlockBinding), v => v.InPlaceEditor);
        }

        private static string GetDisplayText(Folder _, RowPresenter p)
        {
            var result = p.GetValue(_.Path);
            var depth = p.Depth;
            if (depth > 0)
                result = Path.GetFileName(result);
            return result;
        }

        private static string GetPath(Folder _, RowPresenter p, string displayText)
        {
            var directory = GetDirectory(_, p);
            return string.IsNullOrEmpty(directory) ? displayText : Path.Combine(directory, displayText);
        }

        private static string GetDirectory(Folder _, RowPresenter p)
        {
            var result = p.GetValue(_.Path);
            var depth = p.Depth;
            return depth > 0 ? Path.GetDirectoryName(result) : string.Empty;
        }

        private static void Refresh(LargeIconListItemView element, LargeIconListItem _, RowPresenter rowPresenter)
        {
            element.ImageSource = rowPresenter.GetValue(_.LargeIcon);
            element.Text = rowPresenter.GetValue(_.DisplayName);
        }

        public static RowBinding<LargeIconListItemView> AsLargeIconView(this LargeIconListItem _)
        {
            return new RowBinding<LargeIconListItemView>((e, r) => Refresh(e, _, r));
        }
    }
}
