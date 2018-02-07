using DevZest.Data.Presenters;
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
            }).WithInput(TextBox.TextProperty, TextBox.LostFocusEvent, _.Path, v => string.IsNullOrEmpty(v.Text) ? null : v.Text);

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
                result = result.Substring(result.LastIndexOf("\\") + 1);
            return result;
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
