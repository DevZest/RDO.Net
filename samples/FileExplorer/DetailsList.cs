using DevZest.Windows;
using DevZest.Windows.Controls;
using System.Windows;
using System.Windows.Controls;

namespace FileExplorer
{
    public class DetailsList : DataPresenter<DetailsListItem>
    {
        public static readonly string FileSizeTextBlockStyleKey = nameof(FileSizeTextBlockStyleKey);

        protected override void BuildTemplate(TemplateBuilder builder)
        {
            builder
                .GridColumns("20", "Auto", "Auto", "Auto", "Auto")
                .GridRows("20", "20")
                .RowView<RowView>(RowView.SelectableStyle)
                .Layout(Orientation.Vertical)
                .WithSelectionMode(SelectionMode.Single)
                .AddBinding(0, 0, 1, 0, _.DisplayName.ColumnHeader())
                .AddBinding(2, 0, _.FileSize.ColumnHeader())
                .AddBinding(3, 0, _.FileType.ColumnHeader())
                .AddBinding(4, 0, _.DateModified.ColumnHeader())
                .AddBinding(0, 1, _.SmallIcon.Image())
                .AddBinding(1, 1, _.DisplayName.TextBlock())
                .AddBinding(2, 1, _.DateModified.TextBlock("{0:g}"))
                .AddBinding(3, 1, _.FileType.TextBlock())
                .AddBinding(4, 1, _.FileSize.TextBlock("{0:KB}", FileSizeFormatProvider.Singleton).WithStyle((Style)App.Current.FindResource(FileSizeTextBlockStyleKey)));
        }
    }
}
