using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System.Windows;
using System.Windows.Controls;

namespace FileExplorer
{
    public class DetailsList : DataPresenter<DetailsListItem>
    {
        public static readonly StyleKey FileSizeTextBlockStyleKey = new StyleKey(typeof(DetailsList));

        protected override void BuildTemplate(TemplateBuilder builder)
        {
            builder
                .GridColumns("20", "Auto", "Auto", "Auto", "Auto")
                .GridRows("Auto", "20")
                .RowView<RowView>(RowView.SelectableStyleKey)
                .Layout(Orientation.Vertical)
                .WithSelectionMode(SelectionMode.Single)
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
    }
}
