using DevZest.Data.Presenters;
using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.Views;
using System.Windows.Controls;
using DevZest.Data;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace AdventureWorks.SalesOrders
{
    partial class AddressLookupPopup
    {
        private sealed class Presenter : DataPresenter<Address>, RowView.IInputBindingService
        {
            public Presenter(DataView dataView, int? currentAddressID, int customerID)
            {
                CurrentAddressID = currentAddressID;
                CustomerID = customerID;
                dataView.Loaded += OnDataViewLoaded;
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder.GridColumns("150")
                    .GridRows("Auto")
                    .RowView<RowView>(RowView.SelectableStyleKey)
                    .Layout(Orientation.Vertical)
                    .WithSelectionMode(SelectionMode.Single)
                    .AddBinding(0, 0, _.AsAddressBox());
            }

            private Task<DataSet<Address>> LoadDataAsync(CancellationToken ct)
            {
                return Data.GetAddressLookup(CustomerID, ct);
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
                foreach (var row in Rows)
                {
                    if (row.GetValue(_.AddressID) == currentAddressID)
                        return row;
                }
                return null;
            }

            public async void RefreshAsync()
            {
                await RefreshAsync(LoadDataAsync);
                SelectCurrent();
            }

            IEnumerable<InputBinding> RowView.IInputBindingService.InputBindings
            {
                get
                {
                    yield return new InputBinding(SelectCurrentCommand, new KeyGesture(System.Windows.Input.Key.Enter));
                    yield return new InputBinding(SelectCurrentCommand, new MouseGesture(MouseAction.LeftDoubleClick));
                }
            }
        }
    }
}
