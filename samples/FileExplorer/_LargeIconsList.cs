using System;
using DevZest.Windows.Data;
using System.Windows.Controls;
using DevZest.Windows.Controls;

namespace FileExplorer
{
    public class _LargeIconsList : DataPresenter<FolderContent>
    {
        protected override void BuildTemplate(TemplateBuilder builder)
        {
            builder
                .GridColumns("85", "5")
                .GridRows("Auto", "5")
                .Layout(Orientation.Vertical, 0)
                .WithSelectionMode(SelectionMode.Single)
                .AddBinding(0, 0, _.LargeIconView());
        }
    }
}
