# RowPresenter

RDO.WPF automatically maps a collection of <xref:DevZest.Data.DataRow> objects into a collection of <xref:DevZest.Data.Presenters.RowPresenter> objects. <xref:DevZest.Data.Presenters.RowPresenter> contains row level presentation logic which can be consumed by view elements such as <xref:DevZest.Data.Views.RowView>, or can be used as data binding source by providing both data values and view states. <xref:DevZest.Data.Presenters.RowPresenter> does not aware any view elements except holding a reference to its associated <xref:DevZest.Data.Views.RowView>.

## Data Value Access

You can access data values via the following APIs of <xref:DevZest.Data.Presenters.RowPresenter> class:

| API | Description |
|-----|-------------|
| <xref:DevZest.Data.Presenters.RowPresenter.GetValue*> | Gets the data value from specified column. |
| <xref:DevZest.Data.Presenters.RowPresenter.EditValue*> | Enters the editing mode and change value for specified column. |

These APIs can be used to provide data values as data binding source.

## Row Level View States

Each <xref:DevZest.Data.Presenters.RowPresenter> object contains the following row level view states:

| Property | Description |
|----------|-------------|
| <xref:DevZest.Data.Presenters.RowPresenter.IsSelected> | Indicates whether this row is selected. |
| <xref:DevZest.Data.Presenters.RowPresenter.IsCurrent> | Indicates whether this row is current row. Only current row can be in editing mode. |
| <xref:DevZest.Data.Presenters.RowPresenter.IsEditing> | Indicates whether this row is in editing mode. Editing of the row can be saved or cancelled. If this property is true, the <xref:DevZest.Data.Presenters.RowPresenter.IsCurrent> property must also be true.  |
| <xref:DevZest.Data.Presenters.RowPresenter.IsVirtual> | Indicates whether this is a virtual row for inserting. Normally this row is displayed as an empty row at the end of the data grid. |
| <xref:DevZest.Data.Presenters.RowPresenter.IsInserting> | Indicates whether this row is in inserting mode. If this property is true, the property of <xref:DevZest.Data.Presenters.RowPresenter.IsVirtual> and <xref:DevZest.Data.Presenters.RowPresenter.IsEditing> are also be true. |
| <xref:DevZest.Data.Presenters.RowPresenter.IsExpanded> | Indicates whether this row is expanded with its <xref:DevZest.Data.Presenters.RowPresenter.Children>. |

These view states, together with others, can have different presentations, either via data binding or directly consumed by specific view element such as <xref:DevZest.Data.Views.RowView>. For example, the following shows the different presentations for <xref:DevZest.Data.Presenters.RowPresenter.IsSelected> property:

![image](/images/is_selected_presentations.jpg)
