# DataPresenter/SimplePresenter

Data presenter is your class derived from <xref:DevZest.Data.Presenters.DataPresenter`1>, which contains your presentation logic. Your data presenter class should:

* Add scalar data if necessary.
* Implement the abstract <xref:DevZest.Data.Presenters.DataPresenter`1.BuildTemplate*> method, which takes a <xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder> parameter.
* Optionally implement service required by the view.
* Call <xref:DevZest.Data.Presenters.DataPresenter`1.Show*> method of your presenter class to show data to the view.

You can access all data and view states via your data presenter after data shown.

You can also derive your presenter class from <xref:DevZest.Data.Presenters.SimplePresenter>, which is a simplified <xref:DevZest.Data.Presenters.DataPresenter`1>, contains no underlying DataSet but scalar data.

## Scalar Data

A <xref:DevZest.Data.Presenters.Scalar`1> object contains a single value via its <xref:DevZest.Data.Presenters.Scalar`1.Value> property, and can be used as binding source of a binding factory extension method like a column object, which returns <xref:DevZest.Data.Presenters.ScalarBinding`1> instead of <xref:DevZest.Data.Presenters.RowBinding`1>.

You can create <xref:DevZest.Data.Presenters.Scalar`1> object in your presenter class, by calling <xref:DevZest.Data.Presenters.BasePresenter.NewScalar*>. You can also create linked <xref:DevZest.Data.Presenters.Scalar`1> object from existing property/field, by calling <xref:DevZest.Data.Presenters.BasePresenter.NewLinkedScalar*>, with name of the property/field, or getter and setter of the property/field.

You can add validator(s) for <xref:DevZest.Data.Presenters.Scalar`1> object via <xref:DevZest.Data.Presenters.Scalar`1.AddValidator*> API.

The following is an example in `ValidationUI` sample (_LoginPresenter.cs):

[!code-csharp[_LoginPresenter](../../../../samples/ValidationUI/_LoginPresenter.cs)]

## Template Builder

In your <xref:DevZest.Data.Presenters.DataPresenter`1.BuildTemplate*> override method, use provided <xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder> parameter as [fluent interface](https://en.wikipedia.org/wiki/Fluent_interface
) to configure your data presenter.

You can add <xref:data_binding> objects into the template:

* <xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder.Layout*>: Defines how the layout expands with row collection. By default without calling this method, the form is columnar for current row; You can call this method to define the layout which expands vertically or horizontally, contained by <xref:DevZest.Data.Views.RowView>. Additionally you can specify `flowRepeatCount` parameter to define layout that rows will flow in <xref:DevZest.Data.Views.BlockView> first, then expand afterwards.
* <xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder.GridColumns*>/<xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder.GridRows*>: Defines a flexible grid area consisting of columns and rows. Data binding objects will later be added to the template for specified grid cell or range.
* <xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder.AddBinding*>: Adds a <xref:data_binding> object into the template, by specifying grid cell or range. Data binding object is returned by calling binding factory extension method, you can further customize the target UI element by calling <xref:DevZest.Data.Presenters.BindingManager.WithStyle*> or <xref:DevZest.Data.Presenters.RowBinding`1.OverrideSetup*>/<xref:DevZest.Data.Presenters.RowBinding`1.OverrideRefresh*>/<xref:DevZest.Data.Presenters.RowBinding`1.OverrideCleanup*> methods.

You can define frozen columns/rows from scrolling using <xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder.WithFrozenLeft*>, <xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder.WithFrozenTop*>, <xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder.WithFrozenRight*> and <xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder.WithFrozenBottom*>. You can also define number of grid rows/columns adheres to the bottom/right most of the view if rows cannot fill the full size of the view.

You can draw grid lines using <xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder.GridLineX*> and <xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder.GridLineY*>.

You can also define row level configurations:

* <xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder.MakeRecursive*>: Defines row collection as self recursive to show tree view like UI. This will automatically populate <xref:DevZest.Data.Presenters.RowPresenter.Children> property of <xref:DevZest.Data.Presenters.RowPresenter> object. You can then expand/collapse <xref:DevZest.Data.Presenters.RowPresenter> via <xref:DevZest.Data.Presenters.RowPresenter.ToggleExpandState*> API.
* <xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder.RowRange*>: Defines grid range for <xref:DevZest.Data.Views.RowView>. By default without calling this method, the range will be calculated automatically based on added row bindings.
* <xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder.WithVirtualRowPlacement*>: Defines the virtual row (empty row for adding) placement.
* <xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder.WithSelectionMode*>: Defines the row selection mode.
* <xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder.BlockView*>/<xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder.RowView*>: Customizes <xref:DevZest.Data.Views.BlockView>/<xref:DevZest.Data.Views.RowView> to apply a style (see <xref:view_styles>).
* <xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder.AddBehavior*>: Adds a behavior to <xref:DevZest.Data.Views.BlockView>/<xref:DevZest.Data.Views.RowView>. A behavior is a <xref:DevZest.Data.Presenters.BlockViewBehavior>/<xref:DevZest.Data.Presenters.RowViewBehavior> object which can dynamically affect the look-and-feel of <xref:DevZest.Data.Views.BlockView>/<xref:DevZest.Data.Views.RowView>, for example, <xref:DevZest.Data.Presenters.RowViewAlternation> can set different background colors for odd and even rows.

Other validation related APIs will be discussed in <xref:rdo_wpf_validation> topic.

To see examples of how these APIs are used, in the source code repo, open `RDO.WPF.sln` in Visual Studio, find the source code of the API, then press `CTRL-R,K` to find references of the API.

## Service Implementation

Some view components have customizable presentation logic. Since presentation logic should be implemented in data presenter, it is represented as <xref:DevZest.Data.Presenters.IService> derived interface, and your data presenter can implement this interface to replace the default implementation. Taking <xref:DevZest.Data.Views.ColumnHeader> as an example, it defines an `ColumnHeader.ISortService` interface to sort the data:

```csharp
public class ColumnHeader : ...
{
    public interface ISortService : IService
    {
        IReadOnlyList<IColumnComparer> OrderBy { get; set; }
    }
    ...
}
```

The default implementation sorts the local <xref:DevZest.Data.Presenters.RowPresenter> objects. Your data presenter can implement this interface to reload sorted data from the server, as shown in `AdventureWorksLT.WpfApp` sample (MainWindow.Presenter.cs/MainWindow.Presenter.vb):

# [C#](#tab/cs)

```csharp
partial class MainWindow
{
    ...
    private class Presenter : DataPresenter<SalesOrderHeader>, ColumnHeader.ISortService
    {
        ...
        private IReadOnlyList<IColumnComparer> _orderBy = new IColumnComparer[] { };
        IReadOnlyList<IColumnComparer> ColumnHeader.ISortService.OrderBy
        {
            get { return _orderBy; }
            set
            {
                _orderBy = value;
                RefreshAsync();
            }
        }

        private Task<DataSet<SalesOrderHeader>> LoadDataAsync(CancellationToken ct)
        {
            return App.ExecuteAsync(db => db.GetSalesOrderHeadersAsync(SearchText, _orderBy, ct));
        }

        public Task RefreshAsync()
        {
            return RefreshAsync(LoadDataAsync);
        }
        ...
    }
}
```

# [VB.Net](#tab/vb)

```vb
Partial Class MainWindow
    ...
    Private Class Presenter
        Inherits DataPresenter(Of SalesOrderHeader)
        Implements ColumnHeader.ISortService

        ...
        Private _orderBy As IReadOnlyList(Of IColumnComparer) = New IColumnComparer() {}
        Overloads Property OrderBy() As IReadOnlyList(Of IColumnComparer) Implements ColumnHeader.ISortService.OrderBy
            Get
                Return _orderBy
            End Get
            Set
                _orderBy = Value
                RefreshAsync()
            End Set
        End Property

        Private Function LoadDataAsync(ct As CancellationToken) As Task(Of DataSet(Of SalesOrderHeader))
            Return App.ExecuteAsync(Function(db) db.GetSalesOrderHeadersAsync(SearchText, _orderBy, ct))
        End Function

        Public Overloads Function RefreshAsync() As Task
            Return RefreshAsync(AddressOf LoadDataAsync)
        End Function
        ...
    End Class
End Class
```

***

## Consume DataPresenter

Your data presenter object derived from <xref:DevZest.Data.Presenters.DataPresenter`1> class contains rich set of properties, methods and events which you can consume after shown in DataView. Keep in mind you should NEVER manipulate your view - your data presenter object is the answer to all:

* Show/refresh data: <xref:DevZest.Data.Presenters.DataPresenter`1.Show*>, <xref:DevZest.Data.Presenters.DataPresenter`1.ShowAsync*>, <xref:DevZest.Data.Presenters.DataPresenter`1.Refresh*>, <xref:DevZest.Data.Presenters.DataPresenter`1.RefreshAsync*>, <xref:DevZest.Data.Presenters.DataPresenter`1.ShowOrRefresh*> and <xref:DevZest.Data.Presenters.DataPresenter`1.ShowOrRefreshAsync*>.
* Model: <xref:DevZest.Data.Presenters.DataPresenter`1._>, <xref:DevZest.Data.Presenters.DataPresenter`1.Entity>, <xref:DevZest.Data.Presenters.DataPresenter`1.DataSet> and <xref:DevZest.Data.Presenters.DataPresenter.GetSerializer*>
* Template: <xref:DevZest.Data.Presenters.BasePresenter.Template>, <xref:DevZest.Data.Presenters.DataPresenter.LayoutOrientation> and <xref:DevZest.Data.Presenters.DataPresenter.FlowRepeatCount>.
* View: <xref:DevZest.Data.Presenters.BasePresenter.IsMounted>, <xref:DevZest.Data.Presenters.BasePresenter.OnMounted*>, <xref:DevZest.Data.Presenters.BasePresenter.Mounted>, <xref:DevZest.Data.Presenters.DataPresenter.View>, <xref:DevZest.Data.Presenters.DataPresenter.CurrentContainerView>, <xref:DevZest.Data.Presenters.BasePresenter.OnViewChanged*>, <xref:DevZest.Data.Presenters.BasePresenter.OnViewInvalidating*>, <xref:DevZest.Data.Presenters.BasePresenter.OnViewInvalidated*>, <xref:DevZest.Data.Presenters.BasePresenter.OnViewRefreshing*>, <xref:DevZest.Data.Presenters.BasePresenter.InvalidateMeasure*>, <xref:DevZest.Data.Presenters.BasePresenter.InvalidateView*>, <xref:DevZest.Data.Presenters.BasePresenter.SuspendInvalidateView*>, <xref:DevZest.Data.Presenters.BasePresenter.ResumeInvalidateView*>, <xref:DevZest.Data.Presenters.BasePresenter.OnViewChanged*>, <xref:DevZest.Data.Presenters.BasePresenter.OnViewInvalidating*>, <xref:DevZest.Data.Presenters.BasePresenter.OnViewInvalidated*>, <xref:DevZest.Data.Presenters.BasePresenter.OnViewRefreshing*>, <xref:DevZest.Data.Presenters.BasePresenter.OnViewRefreshed>, <xref:DevZest.Data.Presenters.BasePresenter.ViewChanged>, <xref:DevZest.Data.Presenters.BasePresenter.ViewInvalidating>, <xref:DevZest.Data.Presenters.BasePresenter.ViewInvalidated>, <xref:DevZest.Data.Presenters.BasePresenter.ViewRefreshing> and <xref:DevZest.Data.Presenters.BasePresenter.ViewRefreshed>.
* View scrolling management via <xref:DevZest.Data.Presenters.DataPresenter.Scrollable>.
* Scalar data: <xref:DevZest.Data.Presenters.BasePresenter.NewScalar*>, <xref:DevZest.Data.Presenters.BasePresenter.NewLinkedScalar*> and <xref:DevZest.Data.Presenters.BasePresenter.OnValueChanged*>.
* Rows: <xref:DevZest.Data.Presenters.DataPresenter.Rows>, <xref:DevZest.Data.Presenters.DataPresenter.Item*>,  <xref:DevZest.Data.Presenters.DataPresenter.IsRecursive>, <xref:DevZest.Data.Presenters.DataPresenter.ToggleExpandState*> and <xref:DevZest.Data.Presenters.DataPresenter.HasChildren*>.
* Current row: <xref:DevZest.Data.Presenters.DataPresenter.CurrentRow> and <xref:DevZest.Data.Presenters.DataPresenter.OnCurrentRowChanged*>.
* Row selection: <xref:DevZest.Data.Presenters.DataPresenter.PredictSelectionMode*>, <xref:DevZest.Data.Presenters.DataPresenter.Select*>, <xref:DevZest.Data.Presenters.DataPresenter.SelectedRows>, <xref:DevZest.Data.Presenters.DataPresenter.SelectedDataRows> and <xref:DevZest.Data.Presenters.DataPresenter.OnIsSelectedChanged*>.
* Find row by key: <xref:DevZest.Data.Presenters.DataPresenter.CanMatchRow> and <xref:DevZest.Data.Presenters.DataPresenter.Match*>.
* Sort/filter: <xref:DevZest.Data.Presenters.DataPresenter.Where>, <xref:DevZest.Data.Presenters.DataPresenter.OrderBy> and <xref:DevZest.Data.Presenters.DataPresenter.Apply*>.
* Scalar editing via <xref:DevZest.Data.Presenters.BasePresenter.ScalarContainer>: <xref:DevZest.Data.Presenters.ScalarContainer.BeginEdit*>, <xref:DevZest.Data.Presenters.ScalarContainer.CancelEdit*>, <xref:DevZest.Data.Presenters.ScalarContainer.EndEdit*>, <xref:DevZest.Data.Presenters.ScalarContainer.SuspendValueChangedNotification*> and <xref:DevZest.Data.Presenters.ScalarContainer.ResumeValueChangedNotification*>.
* Row editing: <xref:DevZest.Data.Presenters.DataPresenter.VirtualRow>, <xref:DevZest.Data.Presenters.DataPresenter.EditingRow>, <xref:DevZest.Data.Presenters.DataPresenter.InsertingRow>, <xref:DevZest.Data.Presenters.DataPresenter.IsEditing*>, <xref:DevZest.Data.Presenters.DataPresenter.IsInserting*>, <xref:DevZest.Data.Presenters.DataPresenter.BeginInsertBefore*>, <xref:DevZest.Data.Presenters.DataPresenter.BeginInsertAfter*>, <xref:DevZest.Data.Presenters.DataPresenter.QueryCancelEdit*>, <xref:DevZest.Data.Presenters.DataPresenter.QueryEndEdit*>, <xref:DevZest.Data.Presenters.DataPresenter.ConfirmEndEdit*>, <xref:DevZest.Data.Presenters.DataPresenter.OnEdit*> and <xref:DevZest.Data.Presenters.DataPresenter.ConfirmDelete*>.
* <xref:rdo_wpf_validation>.
* Service: <xref:DevZest.Data.Presenters.DataPresenter.ExistsService*> and <xref:DevZest.Data.Presenters.DataPresenter.GetService*>.
