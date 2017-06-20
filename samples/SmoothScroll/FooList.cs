using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System.Windows.Controls;

namespace SmoothScroll
{
    internal class FooList : DataPresenter<Foo>
    {
        protected override void BuildTemplate(TemplateBuilder builder)
        {
            builder
                .GridColumns("*")
                .GridRows("Auto")
                .Layout(Orientation.Vertical)
                .RowView<RowView>(RowView.SelectableStyleKey)
                .WithSelectionMode(SelectionMode.Extended)
                .AddBinding(0, 0, _.AsTextBlock());
        }
    }
}
