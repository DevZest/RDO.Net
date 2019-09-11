# View Hierarchy

As shown in the architecture diagram:

![image](/images/rdo_wpf_view_hierarchy.jpg)

The view elements are organized by the following container UI elements hierarchically:

| Container | Description |
|-----------|-------------|
| <xref:DevZest.Data.Views.DataView> | The root of the view, contains scalar UI elements and other container elements. |
| <xref:DevZest.Data.Views.BlockView> | Contains flowing <xref:DevZest.Data.Views.RowView>. This level is optional. |
| <xref:DevZest.Data.Views.RowView> | Contains UI elements for one row of data. |

Here is an example of hierarchical view with BlockView:

![image](/images/samples_file_explorer_hierarchical_view.jpg)

Here is another example of hierarchical view with scalar elements and without BlockView:

![image](/images/samples_file_explorer_hierarchical_view2.jpg)

You can run your application in debug mode and investigate the view hierarchy in Visual Studio Live Visual Tree tool window:

![image](/images/vs_live_visual_tree.jpg)

The container elements can be bridges between view and presenter. For example, <xref:DevZest.Data.Views.RowView> exposes a <xref:DevZest.Data.Views.RowView.RowPresenter> property. Since all presentation logic are implemented in the presenters, sometimes you may need to invoke these presentation logic from view. In your custom control if you want to access the presenter, you have two options:

* If this control is data binding target, a presenter will be passed in during data binding initialization, which you can save for future use. This is the preferred way.
* If this control is not part of data binding, you can find ancestor container element in the visual tree to access its corresponding presenter.
