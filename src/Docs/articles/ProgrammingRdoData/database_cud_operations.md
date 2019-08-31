# Database CUD operations

In RDO.Data, you can create, update and delete data in <xref:DevZest.Data.DbTable`1>:

| API | Description |
|-----|-------------|
| <xref:DevZest.Data.DbTable`1.InsertAsync*> | Insert from scalar data, DataSet or DbSet. |
| <xref:DevZest.Data.DbTable`1.UpdateAsync*> | Update from scalar data, DataSet or DbSet. |
| <xref:DevZest.Data.DbTable`1.DeleteAsync*> | Delete by condition, or match with DataSet or DbSet. |

## Inserting Data

You can insert into <xref:DevZest.Data.DbTable`1> via <xref:DevZest.Data.DbTable`1.InsertAsync*> API, from various of data sources:

* Scalar data: Scalar data is represented by <xref:expression_column>. You can use <xref:DevZest.Data.ColumnMapper> class to map between scalar data column and target DbTable column. If all columns of target DbTable has default values, you can insert a row without any data specified. Optionally you can provide a `<Action int?>` delegate to receive newly generated identity column value for current insert.
* DataSet: A <xref:DevZest.Data.DataSet`1> object can be used as source data for inserting. The generated native SQL for inserting from DataSet is database specific. For SQL Server and MySQL, multiple row DataSet will be serialized into JSON and passed to database server as single param for inserting, all done in one server round trip for best performance. To support this feature, SQL Server requires version 2016 (13) or later, MySQL requires version 8.0.4 or later. For newly generated identity column values, you can specify a optional boolean value parameter to indicate whether these values should be updated back to the source DataSet. This feature is especially useful if you're inserting parent-child hierarchical DataSet.
* DbSet: A <xref:DevZest.Data.DbTable`1> or <xref:DevZest.Data.DbQuery`1> can be used as source data for inserting. Note <xref:DevZest.Data.DbTable`1> object can be temporary table.

A sample code of inserting data:

# [C#](#tab/cs)

```cs
partial class Db
{
    ...
    public async Task<int?> CreateSalesOrderAsync(DataSet<SalesOrderInfo> salesOrders, CancellationToken ct)
    {
        await EnsureConnectionOpenAsync(ct);
        using (var transaction = BeginTransaction())
        {
            ...
            await SalesOrderHeader.InsertAsync(salesOrders, true, ct);
            var salesOrderDetails = salesOrders.GetChild(_ => _.SalesOrderDetails);
            ...
            await SalesOrderDetail.InsertAsync(salesOrderDetails, ct);

            await transaction.CommitAsync(ct);
            return salesOrders.Count > 0 ? salesOrders._.SalesOrderID[0] : null;
        }
    }
    ...
}
```

# [VB.Net](#tab/vb)

```vb
Partial Class Db
    ...
    Public Async Function CreateSalesOrderAsync(salesOrders As DataSet(Of SalesOrderInfo), ct As CancellationToken) As Task(Of Integer?)
        Await EnsureConnectionOpenAsync(ct)
        Using transaction = BeginTransaction()
            ...
            Await SalesOrderHeader.InsertAsync(salesOrders, True, ct)
            Dim salesOrderDetails = salesOrders.GetChild(Function(x) x.SalesOrderDetails)
            ...
            Await SalesOrderDetail.InsertAsync(salesOrderDetails, ct)
            Await transaction.CommitAsync(ct)
            If salesOrders.Count > 0 Then
                Return salesOrders.Entity.SalesOrderID(0)
            Else
                Return Nothing
            End If
        End Using
    End Function
    ...
End Class
```

***

## Updating Data

You can update <xref:DevZest.Data.DbTable`1> via <xref:DevZest.Data.DbTable`1.UpdateAsync*> API, from various of data sources:

* Scalar data: Scalar data is represented by <xref:expression_column>. You can use <xref:DevZest.Data.ColumnMapper> class to map between scalar data column and target DbTable column. A condition can be provided to filter rows should be updated in the table. If no condition specified, all rows in the table will be updated with the same scalar value.
* DataSet: A <xref:DevZest.Data.DataSet`1> object can be used as source data for updating. The source DataSet and target table must all have primary key, and if there are different you must provide a <xref:DevZest.Data.KeyMapping> between them. The generated native SQL for updating from DataSet is database specific. For SQL Server and MySQL, multiple row DataSet will be serialized into JSON and passed to database server as single param for updating, all done in one server round trip for best performance. To support this feature, SQL Server requires version 2016 (13) or later, MySQL requires version 8.0.4 or later.
* DbSet: A <xref:DevZest.Data.DbTable`1> or <xref:DevZest.Data.DbQuery`1> can be used as source data for inserting. Note <xref:DevZest.Data.DbTable`1> object can be temporary table. The source DbSet and target table must all have primary key, and if there are different you must provide a <xref:DevZest.Data.KeyMapping> between them.

A sample code of updating data:

# [C#](#tab/cs)

```cs
partial class Db
{
    ...
    public async Task UpdateSalesOrderAsync(DataSet<SalesOrderInfo> salesOrders, CancellationToken ct)
    {
        await EnsureConnectionOpenAsync(ct);
        using (var transaction = BeginTransaction())
        {
            await SalesOrderHeader.UpdateAsync(salesOrders, ct);
            await SalesOrderDetail.DeleteAsync(salesOrders, (s, _) => s.Match(_.FK_SalesOrderHeader), ct);
            var salesOrderDetails = salesOrders.GetChild(_ => _.SalesOrderDetails);
            await SalesOrderDetail.InsertAsync(salesOrderDetails, ct);

            await transaction.CommitAsync(ct);
        }
    }
    ...
}

```

# [VB.Net](#tab/vb)

```vb
Partial Class Db
    ...
    Public Async Function UpdateSalesOrderAsync(salesOrders As DataSet(Of SalesOrderInfo), ct As CancellationToken) As Task
        Await EnsureConnectionOpenAsync(ct)
        Using transaction = BeginTransaction()
            ...
            Await SalesOrderHeader.UpdateAsync(salesOrders, ct)
            Await SalesOrderDetail.DeleteAsync(salesOrders, Function(s, x) s.Match(x.FK_SalesOrderHeader), ct)
            Dim salesOrderDetails = salesOrders.GetChild(Function(x) x.SalesOrderDetails)
            ...
            Await SalesOrderDetail.InsertAsync(salesOrderDetails, ct)
            Await transaction.CommitAsync(ct)
        End Using
    End Function
    ...
End Class
```

***

## Deleting Data

You can delete data in <xref:DevZest.Data.DbTable`1> via <xref:DevZest.Data.DbTable`1.DeleteAsync*> API:

* By condition: A condition can be provided to filter rows should be deleted in the table. If no condition specified, all rows in the table will be deleted.
* DataSet: A <xref:DevZest.Data.DataSet`1> object can be used as source data for deleting. The source DataSet and target table must all have primary key, and if there are different you must provide a <xref:DevZest.Data.KeyMapping> between them. The generated native SQL for deleting from DataSet is database specific. For SQL Server and MySQL, multiple row DataSet will be serialized into JSON and passed to database server as single param for deleting, all done in one server round trip for best performance. To support this feature, SQL Server requires version 2016 (13) or later, MySQL requires version 8.0.4 or later.
* DbSet: A <xref:DevZest.Data.DbTable`1> or <xref:DevZest.Data.DbQuery`1> can be used as source data for inserting. Note <xref:DevZest.Data.DbTable`1> object can be temporary table. The source DbSet and target table must all have primary key, and if there are different you must provide a <xref:DevZest.Data.KeyMapping> between them.

A sample code of deleting data:

# [C#](#tab/cs)

```cs
partial class Db
{
    ...
    public Task<int> DeleteSalesOrderAsync(DataSet<SalesOrderHeader.Key> dataSet, CancellationToken ct)
    {
        return SalesOrderHeader.DeleteAsync(dataSet, (s, _) => s.Match(_), ct);
    }
    ...
}
```

# [VB.Net](#tab/vb)

```vb
Partial Class Db
    ...
    Public Function DeleteSalesOrderAsync(dataSet As DataSet(Of SalesOrderHeader.Key), ct As CancellationToken) As Task(Of Integer)
        Return SalesOrderHeader.DeleteAsync(dataSet, Function(s, x) s.Match(x), ct)
    End Function
    ...
End Class
```

***
