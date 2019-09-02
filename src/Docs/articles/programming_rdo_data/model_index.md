# Model Index

A database index is a data structure that improves the speed of data retrieval operations on a database table at the cost of additional writes and storage space to maintain the index data structure. Indexes are used to quickly locate data without having to search every row in a database table every time a database table is accessed. Indexes can be created using one or more columns of a database table, providing the basis for both rapid random lookups and efficient access of ordered records.

An index is a copy of selected columns from a model, being a readonly property which returns a <xref:DevZest.Data.ColumnSort> array, specified by a pair of <xref:DevZest.Data.Annotations.DbIndexAttribute> (at model class level) and <xref:DevZest.Data.Annotations._DbIndexAttribute> (at property level):

# [C#](#tab/cs)

```cs
[DbIndex(nameof(IX_Address_StateProvince), Description = "Nonclustered index.")]
public class Address : ...
{
    ...
    [_DbIndex]
    private ColumnSort[] IX_Address_StateProvince
    {
        get { return new ColumnSort[] { StateProvince }; }
    }
    ...
}
```

# [VB.Net](#tab/vb)

```vb
<DbIndex("IX_Address_StateProvince", Description:="Nonclustered index.")>
Public Class Address
    <_DbIndex>
    Private ReadOnly Property IX_Address_AddressLine1_AddressLine2_City_StateProvince_PostalCode_CountryRegion As ColumnSort()
        Get
            Return New ColumnSort() {AddressLine1, AddressLine2, City, StateProvince, PostalCode, CountryRegion}
        End Get
    End Property
    ...
End Class
```

***

>[!Note]
>As its name suggests, DbIndex affects database server only. Unlike unique constraint, setting <xref:DevZest.Data.Annotations.DbIndexAttribute.IsUnique> property of <xref:DevZest.Data.Annotations.DbIndexAttribute> to true, will not enforce data row unique for dataset.

You can add index by clicking the left top drop down button of Model Visualizer tool window, then click 'Add Index...':

![image](/images/model_visualizer_add_ix.jpg)

The following dialog will be displayed:

![image](/images/model_visualizer_add_ix_dialog.jpg)

Fill the dialog form and click "OK", code of a DbIndex will be generated automatically.
