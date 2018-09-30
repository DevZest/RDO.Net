using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System.Windows.Controls;

namespace FileExplorer
{
    public class LargeIconListPresenter : DirectoryListPresenter<LargeIconListItem>
    {
        public LargeIconListPresenter(DataView directoryListView, DirectoryTreePresenter directoryTree)
            : base(directoryListView, directoryTree)
        {
        }

        public sealed override DirectoryListMode Mode
        {
            get { return DirectoryListMode.LargeIcon; }
        }

        protected override void BuildTemplate(TemplateBuilder builder)
        {
            builder
                .GridColumns("85", "5")
                .GridRows("Auto", "5")
                .Layout(Orientation.Vertical, 0)
                .WithSelectionMode(SelectionMode.Extended)
                .AddBinding(0, 0, _.BindToLargeIconListItemView());
        }
    }
}
