using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System.Windows.Controls;

namespace SmoothScroll
{
    partial class MainWindow
    {
        private class Presenter : DataPresenter<ListItem>
        {
            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder
                    .GridColumns("*")
                    .GridRows("Auto")
                    .Layout(Orientation.Vertical)
                    .RowView<RowView>(RowView.Styles.Selectable)
                    .WithSelectionMode(SelectionMode.Extended)
                    .AddBinding(0, 0, _.AsTextBlock());
            }
        }
    }
}
