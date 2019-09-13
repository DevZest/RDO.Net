# DataPresenter

Data presenter is your class derived from <xref:DevZest.Data.Presenters.DataPresenter`1>, which contains your presentation logic. Your data presenter class should:

* Implement the abstract <xref:DevZest.Data.Presenters.DataPresenter`1.BuildTemplate*> method, which takes a <xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder> parameter.
* Optionally implement service required by the view.
* Call <xref:DevZest.Data.Presenters.DataPresenter`1.Show*> method of your presenter class to show data to the view.

You can access all data and view states via your data presenter after data shown.

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

Other validation related APIs will be discussed in <xref:validation> topic.

To see examples of how these APIs are used, in the source code repo, open `RDO.WPF.sln` in Visual Studio, find the source code of the API, then press `CTRL-R,K` to find references of the API.

## Service Implementation

## View States Access
