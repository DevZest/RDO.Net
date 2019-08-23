# Model Columns

Concrete type derived from <xref:DevZest.Data.Column`1> can be used to represent column of data in the model, which is strongly typed by its type argument. The following column types are provided by the framework:

**Database Agnostic:**

[!include[RDO.Data Columns Agnostic](../_rdo_data_columns_agnostic.md)]

**SQL Server:**

[!include[RDO.Data Columns SQL Server](../_rdo_data_columns_sql_server.md)]

## LocalColumn vs. Others

There are two kind of data columns: <xref:DevZest.Data.LocalColumn`1> and others prefixed with '_'(underscore). As mentioned earlier in <xref:orm_data_access_the_right_way>, it's not feasible to map between relational data and arbitrary objects. <xref:DevZest.Data.LocalColumn`1> is provided as a fallback mechanism for local used data only. The following table compares these two:

| LocalColumn | Others |
|-------------|--------|
| Can map to arbitrary type of data, by providing type argument. | Can only map to specified type of data. For example, <xref:DevZest.Data._String> can only map to data of type System.String |
| Not supported by database providers. Works locally only. | Normally supported by database providers, works both locally and database server side. |
| Serialization/deserialization not supported. | Support JSON serialization/deserialization out-of-box. |
| Computation/expression initialized by delegate. | Computation/expression initialized by expression tree from operator overloading and custom functions. |

## Column Id and Mounter

Column can be identified by its <xref:DevZest.Data.Column.Id> property, a <xref:DevZest.Data.ColumnId> structure that contains the declaring type and name. This property is set during column mounting by calling <xref:DevZest.Data.Model.RegisterColumn*>. <xref:DevZest.Data.Model.RegisterColumn*> can return a <xref:DevZest.Data.Mounter`1> object, which can be used for subsequent <xref:DevZest.Data.Model.RegisterColumn*> calls. Calling <xref:DevZest.Data.Model.RegisterColumn*> with exiting <xref:DevZest.Data.Mounter`1> object will:

* The <xref:DevZest.Data.Column.OriginalId> property will be set to the value of existing mounter. This property will be used to auto-select columns between different models.
* The new mounter will inherit the initialization of the existing mounter, if any.

You can think of calling <xref:DevZest.Data.Model.RegisterColumn*> with exiting <xref:DevZest.Data.Mounter`1> object as column inheritance. Typically, columns that will be stored in database table, should have a corresponding static <xref:DevZest.Data.Mounter`1> field, with name prefixed with underscore(_); key or projection class, by convention nested class of this model, use this <xref:DevZest.Data.Mounter`1> field to reference these table columns.

## Computation Column

Computation column is a powerful feature of RDO.Data. You can define expression for data column so that its value will be calculated automatically, either locally or on the database server side. The framework tracks the calculation dependency and always synchronize the calculated result.

To add a column computation into the model, after adding the column into the model, click the left top drop down button of Model Visualizer tool window, then click 'Add Computation...':

![image](/images/model_visualizer_add_computation.jpg)

The following dialog will be displayed:

![image](/images/model_visualizer_add_computation_dialog.jpg)

Click button 'OK', the following code will be generated automatically, with a pair of <xref:DevZest.Data.Annotations.ComputationAttribute> and <xref:DevZest.Data.Annotations._ComputationAttribute>:

# [C#](#tab/cs)

```cs
[Computation(nameof(ComputeXxx)]
public class ...
{
    ...
    [_Computation]
    private void ComputeXxx()
    {
        throw new global::System.NotImplementedException();
    }
}
```

# [VB.Net](#tab/vb)

```vb
<Computation("ComputeXxx")>
Public Class ...
    ...
    <_Computation>
    Private Sub ComputeXxx()
        Throw New Global.System.NotImplementedException()
    End Sub
End Class
```

***

You should implement the generated `ComputeXxx` method. The following are two examples to compute a normal column and a <xref:DevZest.Data.LocalColumn`1>:

### Normal Column Computation

Normal column can participate the expression directly via operator overloading and custom functions, an expression tree will be generated automatically:

# [C#](#tab/cs)

```cs
[Computation(nameof(ComputeTotalDue))]
public class SalesOrderHeader : ...
{
    ...
    [_Computation]
    private void ComputeTotalDue()
    {
        TotalDue.ComputedAs((SubTotal + TaxAmt + Freight).IfNull(_Decimal.Const(0)));
    }
}
```

# [VB.Net](#tab/vb)

```vb
<Computation("ComputeTotalDue")>
Public Class SalesOrderHeader
    ...
    <_Computation>
    Private Sub ComputeTotalDue()
        TotalDue.ComputedAs((SubTotal + TaxAmt + Freight).IfNull(_Decimal.[Const](0)))
    End Sub
End Class
```

***

### LocalColumn Computation

LocalColumn can be backed by delegate. The dependent columns must be explicitly specified, and the delegate must be static method:

# [C#](#tab/cs)

```cs
[Computation(nameof(ComputeContactPerson))]
public class Customer...
{
    ...
    [_Computation]
    private void ComputeContactPerson()
    {
        ContactPerson.ComputedAs(LastName, FirstName, Title, GetContactPerson, false);
    }

    private static string GetContactPerson(DataRow dataRow, _String lastName, _String firstName, _String title)
    {
        return GetContactPerson(lastName[dataRow], firstName[dataRow], title[dataRow]);
    }
}
```

# [VB.Net](#tab/vb)

```vb
<Computation("ComputeContactPerson")>
Public Class Customer
    ...
    <_Computation>
    Private Sub ComputeContactPerson()
        ContactPerson.ComputedAs(LastName, FirstName, Title, AddressOf GetContactPerson, False)
    End Sub

    Private Shared Function GetContactPerson(ByVal dataRow As DataRow, ByVal lastName As _String, ByVal firstName As _String, ByVal title As _String) As String
        Return GetContactPerson(lastName(dataRow), firstName(dataRow), title(dataRow))
    End Function
End Class
```

***
