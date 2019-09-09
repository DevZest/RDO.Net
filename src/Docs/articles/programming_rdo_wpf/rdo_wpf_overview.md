# RDO.WPF Overview

Built on top of RDO.Data, RDO.WPF can help developers to build WPF desktop applications:

![image](/images/RdoWpfOverview.jpg)

RDO.WPF is implemented as Model-View-Presenter (MVP) to improve the separation of concerns in presentation logic. The following is the architecture of RDO.WPF MVP:

![image](/images/RdoWpfMvpArchitecture.jpg)

* The model contains the data values and data logic such as computation and validation in a <xref:DevZest.Data.DataSet`1> object. The <xref:DevZest.Data.DataSet`1> object contains collection of <xref:DevZest.Data.DataRow> objects and <xref:DevZest.Data.Column> objects, similar as two dimensional array. The model provides events to notify data changes, it does not aware the existence of the presenter at all.
* The view contains UI components which directly interacts with user. These UI components are designed as dumb as possible, all presentation logic are implemented in the presenter. Despite the container UI components such as <xref:DevZest.Data.Views.DataView>, <xref:DevZest.Data.Views.BlockView> and <xref:DevZest.Data.Views.RowView>, most UI elements do not aware the existence of the presenter at all.
* The presenter is the core to tie model and view together, it implements the following presentation logic:
  * Selection, filtering and hierarchical grouping.
  * UI elements life time management and data binding.
  * Editing and validation.
  * Layout and UI virtualization.

Using RDO.WPF, you have ALL of your presentation logic in 100% strongly typed, extremely clean code.
