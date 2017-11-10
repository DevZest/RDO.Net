using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System.Windows.Controls;
using DevZest.Data;
using System;
using System.Collections.Generic;

namespace FileExplorer
{
    public class DetailsList : DataPresenter<DetailsListItem>
    {
        public static readonly StyleId FileSizeTextBlockStyleKey = new StyleId(typeof(DetailsList));

        protected override void BuildTemplate(TemplateBuilder builder)
        {
            builder
                .GridColumns("20", "Auto", "Auto", "Auto", "Auto")
                .GridRows("Auto", "20")
                .RowView<RowView>(RowView.Styles.Selectable)
                .Layout(Orientation.Vertical)
                .WithFrozenTop(1)
                .WithSelectionMode(SelectionMode.Extended)
                .AddBinding(0, 0, 1, 0, _.DisplayName.AsColumnHeader("Name"))
                .AddBinding(2, 0, _.DateModified.AsColumnHeader("Date modified"))
                .AddBinding(3, 0, _.FileType.AsColumnHeader("Type"))
                .AddBinding(4, 0, _.FileSize.AsColumnHeader("Size"))
                .AddBinding(0, 1, _.SmallIcon.AsImage())
                .AddBinding(1, 1, _.DisplayName.AsTextBlock())
                .AddBinding(2, 1, _.DateModified.AsTextBlock("{0:g}"))
                .AddBinding(3, 1, _.FileType.AsTextBlock())
                .AddBinding(4, 1, _.FileSize.AsTextBlock("{0:KB}", FileSizeFormatProvider.Singleton).WithStyle(FileSizeTextBlockStyleKey));
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
