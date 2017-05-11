using DevZest.Windows.Data;

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

        public static RowBinding<FolderView> FolderView(this Folder _)
        {
            return new RowBinding<FolderView>((e, r) => Refresh(e, _, r));
        }
    }
}
