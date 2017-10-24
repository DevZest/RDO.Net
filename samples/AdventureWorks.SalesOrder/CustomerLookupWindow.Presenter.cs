using DevZest.Data.Presenters;
using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.Views;
using System.Windows.Controls;
using DevZest.Data;

namespace AdventureWorks.SalesOrders
{
    partial class CustomerLookupWindow
    {
        private sealed class Presenter : DataPresenter<CustomerToLookup>
        {
            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder.GridColumns("Auto", "Auto", "Auto", "Auto")
                    .GridRows("Auto", "20")
                    .RowView<RowView>(RowView.SelectableStyleKey)
                    .Layout(Orientation.Vertical)
                    .WithFrozenTop(1)
                    .GridLineX(new GridPoint(0, 1), 4)
                    .GridLineY(new GridPoint(1, 1), 1)
                    .GridLineY(new GridPoint(2, 1), 1)
                    .GridLineY(new GridPoint(3, 1), 1)
                    .WithSelectionMode(SelectionMode.Single)
                    .AddBinding(0, 0, _.CompanyName.AsColumnHeader())
                    .AddBinding(1, 0, _.ContactPerson.AsColumnHeader())
                    .AddBinding(2, 0, _.Phone.AsColumnHeader())
                    .AddBinding(3, 0, _.EmailAddress.AsColumnHeader())
                    .AddBinding(0, 1, _.CompanyName.AsTextBlock())
                    .AddBinding(1, 1, _.ContactPerson.AsTextBlock())
                    .AddBinding(2, 1, _.Phone.AsTextBlock())
                    .AddBinding(3, 1, _.EmailAddress.AsTextBlock());
            }
        }
    }
}
