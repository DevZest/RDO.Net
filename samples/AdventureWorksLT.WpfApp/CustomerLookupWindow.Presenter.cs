using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System.Windows.Controls;
using DevZest.Data;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Input;

namespace DevZest.Samples.AdventureWorksLT
{
    partial class CustomerLookupWindow
    {
        private sealed class Presenter : DataPresenter<Customer>, RowView.ICommandService
        {
            public Presenter(DataView dataView, int? currentCustomerID)
            {
                CurrentCustomerID = currentCustomerID;
                dataView.Loaded += OnDataViewLoaded;
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder.GridColumns("200", "200", "120", "190")
                    .GridRows("Auto", "20")
                    .RowView<RowView>(RowView.Styles.Selectable)
                    .Layout(Orientation.Vertical)
                    .WithFrozenTop(1)
                    .GridLineX(new GridPoint(0, 1), 4)
                    .GridLineY(new GridPoint(1, 1), 1)
                    .GridLineY(new GridPoint(2, 1), 1)
                    .GridLineY(new GridPoint(3, 1), 1)
                    .WithSelectionMode(SelectionMode.Single)
                    .AddBinding(0, 0, _.CompanyName.BindToColumnHeader("Company Name"))
                    .AddBinding(1, 0, _.ContactPerson.BindToColumnHeader("Contact Person"))
                    .AddBinding(2, 0, _.Phone.BindToColumnHeader("Phone"))
                    .AddBinding(3, 0, _.EmailAddress.BindToColumnHeader("Email Address"))
                    .AddBinding(0, 1, _.CompanyName.BindToTextBlock())
                    .AddBinding(1, 1, _.ContactPerson.BindToTextBlock())
                    .AddBinding(2, 1, _.Phone.BindToTextBlock())
                    .AddBinding(3, 1, _.EmailAddress.BindToTextBlock());
            }

            private Task<DataSet<Customer>> LoadDataAsync(CancellationToken ct)
            {
                return App.ExecuteAsync(db => db.Customer.ToDataSetAsync(ct));
            }

            public int? CurrentCustomerID { get; private set; }

            private void OnDataViewLoaded(object sender, RoutedEventArgs e)
            {
                var dataView = (DataView)sender;
                dataView.Loaded -= OnDataViewLoaded;
                ShowAsync(dataView);
            }

            private async void ShowAsync(DataView dataView)
            {
                await ShowAsync(dataView, LoadDataAsync);
                SelectCurrent();
            }

            private void SelectCurrent()
            {
                Select(CurrentCustomerID);
            }

            private void Select(int? currentCustomerID)
            {
                if (!currentCustomerID.HasValue)
                    return;

                var current = GetRow(currentCustomerID.Value);
                if (current != null)
                    Select(current, SelectionMode.Single);
            }

            private RowPresenter GetRow(int currentCustomerID)
            {
                return Match(new Customer.PK(currentCustomerID));
            }

            public async void RefreshAsync()
            {
                await RefreshAsync(LoadDataAsync);
                SelectCurrent();
            }

            private string _searchText;
            public string SearchText
            {
                get { return _searchText; }
                set
                {
                    if (_searchText == value)
                        return;

                    _searchText = value;
                    if (string.IsNullOrEmpty(value))
                        Where = null;
                    else
                        Where = dataRow => _.CompanyName[dataRow].Contains(value) ||
                            _.ContactPerson[dataRow].Contains(value) ||
                            _.Phone[dataRow].Contains(value) ||
                            _.EmailAddress[dataRow].Contains(value);
                    SelectCurrent();
                }
            }

            IEnumerable<CommandEntry> RowView.ICommandService.GetCommandEntries(RowView rowView)
            {
                var baseService = this.GetRegisteredService<RowView.ICommandService>();
                foreach (var entry in baseService.GetCommandEntries(rowView))
                    yield return entry;
                yield return Commands.SelectCurrent.Bind(new KeyGesture(System.Windows.Input.Key.Enter), new MouseGesture(MouseAction.LeftDoubleClick));
            }
        }
    }
}
