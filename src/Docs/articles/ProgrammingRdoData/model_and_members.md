# Model and Members

## Model

Your business models are classes derived from <xref:DevZest.Data.Model> or <xref:DevZest.Data.Model`1>. Your model class defines data schema and other data logic which can be shared across different data sources. A model will be created and associated with one of the following <xref:DevZest.Data.DataSource> automatically:

* <xref:DevZest.Data.DataSet`1>: In-memory collection of data;
* <xref:DevZest.Data.DbTable`1>: Database table, either permanent or temporary;
* <xref:DevZest.Data.DbQuery`1>: Database query which can be executed on database server.

Your model class must have a parameterless constructor.

## Model Members

Your model can consist the following members, as read only properties:

| Model Member | Description |
|--------------|-------------|
| Column | Property of concrete type derived from <xref:DevZest.Data.Column`1> to represents a column of data. See <xref:model_column> for more information. |
| <xref:DevZest.Data.ColumnList`1> | Columns accessed via index, can be used for pivot table of data. |
| Projection | Property of type derived from <xref:DevZest.Data.Projection> to represent lookup data from other model. See <xref:model_projection> for more information. |
| Child Model | Represents hierarchical of data. |

## Member Registration

Model members should be registered so that the property can be properly mounted, otherwise the property will always return null value. The following registration methods should be invoked, either in static class constructor or as static field initializer:

* <xref:DevZest.Data.Model.RegisterColumn*>/<xref:DevZest.Data.Model.RegisterLocalColumn*>
* <xref:DevZest.Data.Model.RegisterColumnList*>
* <xref:DevZest.Data.Model.RegisterProjection*>
* <xref:DevZest.Data.Model.RegisterChildModel*>

Model member without registration will generate a compile time warning. With RDO.Tools installed, you can fix this warning by moving the caret to the property name, and pressing *CTRL-.* in Visual Studio:

# [C#](#tab/cs)

![image](/images/tutorial_add_mounter_cs.jpg)

# [VB.Net](#tab/vb)

![image](/images/tutorial_add_mounter_vb.jpg)

***

## Model Visualizer

Model Visualizer is your best friend to manipulate model members. You can show Model Visualizer tool window by clicking menu "View" -> "Other Windows" -> "Model Visualizer" in Visual Studio:

![image](/images/tutorial_model_visualizer_empty_movie.jpg)

To add model member, in Model Visualizer tool window, click the left top ![image](/images/model_visualizer_add.jpg) button, the following code snippet will be inserted:

# [C#](#tab/cs)

![image](/images/model_visualizer_add_property_cs.jpg)

# [VB.Net](#tab/vb)

![image](/images/model_visualizer_add_property_vb.jpg)

***

Tabbing through the code snippet to change the property name and property type. When done, press ESC to quit code snippet editing. You now have a compile-time warning `Missing registration for property '<Name>'`. You can fix this warning by moving the caret to the property name, and pressing *CTRL-.* in Visual Studio as described previously.

More features of Model Visualizer tool window will be discussed in later topics. Keep in mind Model Visualizer is a great place to start your model design, you can add EVERYTHING via Model Visualizer tool window, without remembering lots of APIs.
