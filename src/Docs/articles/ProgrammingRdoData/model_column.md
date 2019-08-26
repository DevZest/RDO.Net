# Model Column

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

## Column Annotations

Model column can be decorated with annotations which are attributes derived from <xref:DevZest.Data.Annotations.Primitives.ColumnAttribute> and <xref:DevZest.Data.Annotations.Primitives.ValidationColumnAttribute>, such as:

* Naming and descriptive: [DbColumn](xref:DevZest.Data.Annotations.DbColumnAttribute), [Display](xref:DevZest.Data.Annotations.DisplayAttribute);
* Value and identity: [DefaultValue](xref:DevZest.Data.Annotations.DefaultValueAttribute), [AutoDateTime](xref:DevZest.Data.Annotations.AutoDateTimeAttribute), [AutoGuid](xref:DevZest.Data.Annotations.AutoGuidAttribute), [Identity](xref:DevZest.Data.Annotations.IdentityAttribute)/[Identity16](xref:DevZest.Data.Annotations.Identity16Attribute)/[Identity64](xref:DevZest.Data.Annotations.Identity64Attribute);
* Validators: [Required](xref:DevZest.Data.Annotations.RequiredAttribute), [StringLength](xref:DevZest.Data.Annotations.StringLengthAttribute), [RegularExpression](xref:DevZest.Data.Annotations.RegularExpressionAttribute), [MaxLength](xref:DevZest.Data.Annotations.MaxLengthAttribute), [EmailAddress](xref:DevZest.Data.Annotations.EmailAddressAttribute), [CreditCard](xref:DevZest.Data.Annotations.CreditCardAttribute), [Url](xref:DevZest.Data.Annotations.UrlAttribute).
* SQL Server data types: [SqlBinary](xref:DevZest.Data.SqlServer.SqlBinaryAttribute), [SqlBinaryMax](xref:DevZest.Data.SqlServer.SqlBinaryMaxAttribute), [SqlChar](xref:DevZest.Data.SqlServer.SqlCharAttribute), [SqlCharMax](xref:DevZest.Data.SqlServer.SqlCharMaxAttribute), [SqlDate](xref:DevZest.Data.SqlServer.SqlDateAttribute), [SqlDateTime](xref:DevZest.Data.SqlServer.SqlDateTimeAttribute), [SqlDateTime2](xref:DevZest.Data.SqlServer.SqlDateTime2Attribute), [SqlDecimal](xref:DevZest.Data.SqlServer.SqlDecimalAttribute), [SqlMoney](xref:DevZest.Data.SqlServer.SqlMoneyAttribute), [SqlNChar](xref:DevZest.Data.SqlServer.SqlNCharAttribute), [SqlNCharMax](xref:DevZest.Data.SqlServer.SqlNCharMaxAttribute), [SqlNVarChar](xref:DevZest.Data.SqlServer.SqlNVarCharAttribute), [SqlNVarCharMax](xref:DevZest.Data.SqlServer.SqlNVarCharMaxAttribute), [SqlSmallDateTime](xref:DevZest.Data.SqlServer.SqlSmallDateTimeAttribute), [SqlSmallMoney](xref:DevZest.Data.SqlServer.SqlSmallMoneyAttribute), [SqlTime](xref:DevZest.Data.SqlServer.SqlTimeAttribute), [SqlTimeStamp](xref:DevZest.Data.SqlServer.SqlTimeStampAttribute), [SqlVarBinary](xref:DevZest.Data.SqlServer.SqlVarBinaryAttribute), [SqlVarBinaryMax](xref:DevZest.Data.SqlServer.SqlVarBinaryMaxAttribute), [SqlVarChar](xref:DevZest.Data.SqlServer.SqlVarCharAttribute), [SqlVarCharMax](xref:DevZest.Data.SqlServer.SqlVarCharMaxAttribute);
* MySQL data types: [MySqlBinary](xref:DevZest.Data.MySql.MySqlBinaryAttribute), [MySqlBlob](xref:DevZest.Data.MySql.MySqlBlobAttribute) [MySqlChar](xref:DevZest.Data.MySql.MySqlCharAttribute), [MySqlDate](xref:DevZest.Data.MySql.MySqlDateAttribute), [MySqlDateTime](xref:DevZest.Data.MySql.MySqlDateTimeAttribute), [MySqlDecimal](xref:DevZest.Data.MySql.MySqlDecimalAttribute), [MySqlLongBlob](xref:DevZest.Data.MySql.MySqlLongBlobAttribute), [MySqlLongText](xref:DevZest.Data.MySql.MySqlLongTextAttribute), [MySqlMediumBlob](xref:DevZest.Data.MySql.MySqlMediumBlobAttribute), [MySqlMediumText](xref:DevZest.Data.MySql.MySqlMediumTextAttribute), [MySqlMoney](xref:DevZest.Data.MySql.MySqlMoneyAttribute), [MySqlText](xref:DevZest.Data.MySql.MySqlTextAttribute), [MySqlTime](xref:DevZest.Data.MySql.MySqlTimeAttribute), [MySqlTimeStamp](xref:DevZest.Data.MySql.MySqlTimeStampAttribute), [MySqlTinyBlob](xref:DevZest.Data.MySql.MySqlTinyBlobAttribute), [MySqlTinyText](xref:DevZest.Data.MySql.MySqlTinyTextAttribute), [MySqlVarBinary](xref:DevZest.Data.MySql.MySqlVarBinaryAttribute), [MySqlVarChar](xref:DevZest.Data.MySql.MySqlVarCharAttribute).

You can add column annotations via Model Visualizer tool window: In *Model Visualizer* tool window, right click the column to display available annotations as context menu:

![image](/images/tutorial_add_identity.jpg)

The above screenshot demonstrates adding a [Identity](xref:DevZest.Data.Annotations.IdentityAttribute) annotation for the `ID` column.

>[!Note]
>Internally, RDO.Tools knows the relationship between annotations and column so that only available annotations will be displayed. It's also smart enough to display contradictive annotations, for example, only one SQL Sever data type annotation can be applied to a given column. It's more than just saving you couple of key strokes.

You're encouraged to develop your own custom column annotation, make sure it's decorated with [ModelDesignerSpec](xref:DevZest.Data.Annotations.Primitives.ModelDesignerSpecAttribute) so that your custom annotation will be friendly to Model Visualizer.
