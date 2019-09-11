# View Hierarchy

As shown in the architecture diagram:

![image](/images/rdo_wpf_view_hierarchy.jpg)

The view elements are organized by the following container UI elements hierarchically:

| Container | Description |
|-----------|-------------|
| <xref:DevZest.Data.Views.DataView> | The root of the view, contains scalar UI elements and other container elements. |
| <xref:DevZest.Data.Views.BlockView> | Contains flowing <xref:DevZest.Data.Views.RowView>. This level is optional. |
| <xref:DevZest.Data.Views.RowView> | Contains UI elements for one row of data. |

Unlike other UI elements which are explicitly created via data binding, <xref:DevZest.Data.Views.BlockView> and <xref:DevZest.Data.Views.RowView> elements are created implicitly. These container elements can be bridges between view and presenter. For example, <xref:DevZest.Data.Views.RowView> exposes a <xref:DevZest.Data.Views.RowView.RowPresenter> property.

Since presenting collection of data is extensively and exclusively supported by the presenter, all the complex controls (controls derived from `System.Windows.Controls.ItemsControl` such as `ListBox` or `DataGrid`) are not necessary any more. By using RDO.WPF, your application only need to deal with simple controls such as `TextBlock` or `TextBox`, via data binding, in an unified way.

Here is an example of hierarchical view with BlockView:

![image](/images/samples_file_explorer_hierarchical_view.jpg)

Here is another example of hierarchical view with scalar elements and without BlockView:

![image](/images/samples_file_explorer_hierarchical_view2.jpg)

You can run your application in debug mode and investigate the view hierarchy in Visual Studio Live Visual Tree tool window:

![image](/images/vs_live_visual_tree.jpg)
