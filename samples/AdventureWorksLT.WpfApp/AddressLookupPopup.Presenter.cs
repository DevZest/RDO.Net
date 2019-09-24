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
    partial class AddressLookupPopup
    {
        public static class Commands
        {
            public static RoutedUICommand SelectCurrent { get { return ApplicationCommands.Open; } }
        }

        private sealed class Presenter : DataPresenter<Address>, RowView.ICommandService
        {
            public Presenter(DataView dataView, int? currentAddressID, int customerID)
            {
                CurrentAddressID = currentAddressID;
                CustomerID = customerID;
                dataView.Loaded += OnDataViewLoaded;
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder.GridColumns("120")
                    .GridRows("Auto")
                    .RowView<RowView>(RowView.Styles.Selectable)
                    .Layout(Orientation.Vertical)
                    .WithSelectionMode(SelectionMode.Single)
                    .AddBinding(0, 0, _.BindToAddressBox());
            }

            private Task<DataSet<Address>> LoadDataAsync(CancellationToken ct)
            {
                return App.ExecuteAsync(db => db.GetAddressLookupAsync(CustomerID, ct));
            }

            public int? CurrentAddressID { get; private set; }

            public int CustomerID { get; private set; }

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
                Select(CurrentAddressID);
            }

            private void Select(int? currentAddressID)
            {
                if (!currentAddressID.HasValue)
                    return;

                var current = GetRow(currentAddressID.Value);
                if (current != null)
                {
                    View.UpdateLayout();
                    Select(current, SelectionMode.Single);
                }
            }

            private RowPresenter GetRow(int currentAddressID)
            {
                return Match(new Address.PK(currentAddressID));
            }

            public async void RefreshAsync()
            {
                await RefreshAsync(LoadDataAsync);
                SelectCurrent();
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
