using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System.Windows.Controls;
using DevZest.Data;
using System;
using System.Collections.Generic;

namespace FileExplorer
{
    public class DetailsListPresenter : DirectoryListPresenter<DetailsListItem>
    {
        public static readonly StyleId FileSizeTextBlockStyleKey = new StyleId(typeof(DetailsListPresenter));

        public DetailsListPresenter(DataView directoryListView, DirectoryTreePresenter directoryTree)
            : base(directoryListView, directoryTree)
        {
        }

        public sealed override DirectoryListMode Mode
        {
            get { return DirectoryListMode.Details; }
        }

        protected override void BuildTemplate(TemplateBuilder builder)
        {
            builder
                .GridColumns("20", "Auto", "Auto", "Auto", "Auto")
                .GridRows("Auto", "20")
                .RowView<RowView>(RowView.Styles.Selectable)
                .Layout(Orientation.Vertical)
                .WithFrozenTop(1)
                .WithSelectionMode(SelectionMode.Extended)
                .AddBinding(0, 0, 1, 0, _.DisplayName.BindToColumnHeader("Name"))
                .AddBinding(2, 0, _.DateModified.BindToColumnHeader("Date modified"))
                .AddBinding(3, 0, _.FileType.BindToColumnHeader("Type"))
                .AddBinding(4, 0, _.FileSize.BindToColumnHeader("Size"))
                .AddBinding(0, 1, _.SmallIcon.BindToImage())
                .AddBinding(1, 1, _.DisplayName.BindToTextBox().MergeIntoInPlaceEditor(_.DisplayName.BindToTextBlock()))
                .AddBinding(2, 1, _.DateModified.BindToTextBlock("{0:g}"))
                .AddBinding(3, 1, _.FileType.BindToTextBlock())
                .AddBinding(4, 1, _.FileSize.BindToTextBlock("{0:KB}", FileSizeFormatProvider.Singleton).WithStyle(FileSizeTextBlockStyleKey));
        }

        public override void Apply(Predicate<DataRow> where, IComparer<DataRow> orderBy)
        {
            base.Apply(where, InsertOrderByType(orderBy));
        }

        private IComparer<DataRow> InsertOrderByType(IComparer<DataRow> orderBy)
        {
            if (orderBy == null || orderBy == OrderBy)
                return orderBy;

            IComparer<DataRow> result = DataRow.OrderBy(_.Type, SortDirection.Ascending);
            return result.ThenBy(orderBy);
        }
    }
}
