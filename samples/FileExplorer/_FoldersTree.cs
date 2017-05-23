﻿using DevZest.Windows;
using System.Windows.Controls;

namespace FileExplorer
{
    public class _FoldersTree : DataPresenter<Folder>
    {
        protected override void BuildTemplate(TemplateBuilder builder)
        {
            builder
                .MakeRecursive(_.SubFolders)
                .GridColumns("Auto")
                .GridRows("Auto")
                .Layout(Orientation.Vertical)
                .WithSelectionMode(SelectionMode.Single)
                .AddBinding(0, 0, _.FolderView());
        }

        protected override void ToggleExpandState(RowPresenter rowPresenter)
        {
            var dataRow = rowPresenter.DataRow;
            Folder.Expand(dataRow);
            base.ToggleExpandState(rowPresenter);
        }
    }
}
