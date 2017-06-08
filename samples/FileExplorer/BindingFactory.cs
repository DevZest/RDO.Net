using DevZest.Windows;

namespace FileExplorer
{
    public static class BindingFactory
    {
        private static void Refresh(FolderView element, Folder _, RowPresenter rowPresenter)
        {
            var depth = rowPresenter.Depth;
            element.Depth = depth;
            element.ImageSource = depth == 0 ? Images.DiskDrive : Images.Folder;
            var text = rowPresenter.GetValue(_.Path);
            if (depth > 0)
                text = text.Substring(text.LastIndexOf("\\") + 1);
            element.Text = text;
        }

        public static RowBinding<FolderView> AsFolderView(this Folder _)
        {
            return new RowBinding<FolderView>((e, r) => Refresh(e, _, r));
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
