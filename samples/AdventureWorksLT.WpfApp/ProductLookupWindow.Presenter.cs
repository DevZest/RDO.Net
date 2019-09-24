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
    partial class ProductLookupWindow
    {
        private sealed class Presenter : DataPresenter<Product>, RowView.ICommandService
        {
            public Presenter(DataView dataView, int? currentProductID)
            {
                CurrentProductID = currentProductID;
                dataView.Loaded += OnDataViewLoaded;
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder.GridColumns("Auto", "*", "Auto")
                    .GridRows("Auto", "20")
                    .RowView<RowView>(RowView.Styles.Selectable)
                    .Layout(Orientation.Vertical)
                    .WithFrozenTop(1)
                    .GridLineX(new GridPoint(0, 1), 3)
                    .GridLineY(new GridPoint(1, 1), 1)
                    .GridLineY(new GridPoint(2, 1), 1)
                    .GridLineY(new GridPoint(3, 1), 1)
                    .WithSelectionMode(SelectionMode.Single)
                    .AddBinding(0, 0, _.ProductNumber.BindToColumnHeader("Product Number"))
                    .AddBinding(1, 0, _.Name.BindToColumnHeader("Name"))
                    .AddBinding(2, 0, _.ListPrice.BindToColumnHeader("List Price"))
                    .AddBinding(0, 1, _.ProductNumber.BindToTextBlock())
                    .AddBinding(1, 1, _.Name.BindToTextBlock())
                    .AddBinding(2, 1, _.ListPrice.BindToTextBlock("{0:C}").WithStyle(MainWindow.Styles.RightAlignedTextBlock));
            }

            private Task<DataSet<Product>> LoadDataAsync(CancellationToken ct)
            {
                return App.ExecuteAsync(db => db.Product.ToDataSetAsync(ct));
            }

            public int? CurrentProductID { get; private set; }

            private void OnDataViewLoaded(object sender, RoutedEventArgs e)
            {
                var dataView = (DataView)sender;
                dataView.Loaded -= OnDataViewLoaded;
                ShowAsync(dataView);
            }

            private async void ShowAsync(DataView dataView)
            {
                await ShowAsync(dataView, LoadDataAsync);
                if (View.DataLoadState == DataLoadState.Succeeded)
                    SelectCurrent();
            }

            private void SelectCurrent()
            {
                Select(CurrentProductID);
            }

            private void Select(int? currentProductID)
            {
                if (!currentProductID.HasValue)
                    return;

                var current = GetRow(currentProductID.Value);
                if (current != null)
                    Select(current, SelectionMode.Single);
            }

            private RowPresenter GetRow(int currentProductID)
            {
                return Match(new Product.PK(currentProductID));
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
                        Where = dataRow => _.ProductNumber[dataRow].Contains(value) || _.Name[dataRow].Contains(value);
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
