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
                .AddBinding(0, 0, 1, 0, _.DisplayName.AsColumnHeader())
                .AddBinding(2, 0, _.FileSize.AsColumnHeader())
                .AddBinding(3, 0, _.FileType.AsColumnHeader())
                .AddBinding(4, 0, _.DateModified.AsColumnHeader())
                .AddBinding(0, 1, _.SmallIcon.AsImage())
                .AddBinding(1, 1, _.DisplayName.AsTextBlock())
                .AddBinding(2, 1, _.DateModified.AsTextBlock("{0:g}"))
                .AddBinding(3, 1, _.FileType.AsTextBlock())
                .AddBinding(4, 1, _.FileSize.AsTextBlock("{0:KB}", FileSizeFormatProvider.Singleton).WithStyle((Style)App.Current.FindResource(FileSizeTextBlockStyleKey)));
        }
    }
}
