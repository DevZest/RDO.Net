# Database Query

Represented by a <xref:DevZest.Data.DbQuery`1> object, database query encapsulates a <xref:DevZest.Data.Primitives.DbQueryStatement> object which can be translated to native SQL by database session provider.

## Simple Query

You can make simple query by invoking the following methods of <xref:DevZest.Data.DbSet`1> class, which is the base class for both <xref:DevZest.Data.DbTable`1> and <xref:DevZest.Data.DbQuery`1>:

* <xref:DevZest.Data.DbSet`1.Where*>: Filtering, to extract only those records that fulfill a specified condition.
* <xref:DevZest.Data.DbSet`1.OrderBy*>: Sorting the result-set in ascending or descending order, optionally you can specify `offset` and `fetch` parameters to limit the number of rows to be returned by the query.
* <xref:DevZest.Data.DbSet`1.Union*>/<xref:DevZest.Data.DbSet`1.UnionAll*>: Combining two <xref:DevZest.Data.DbSet`1>.

The following code:

```cs
using (var db = new Db(...))
{
    var query = db.SalesOrderHeader
        .Where(x => x.SalesOrderID == _Int32.Const(71774) | x.SalesOrderID == _Int32.Const(71776))
        .OrderBy(x => x.SalesOrderID);
}
```

will be translated to the following TSQL which can be examined by `query.ToString()`:

```sql
SELECT
    [SalesOrderHeader].[SalesOrderID] AS [SalesOrderID],
    [SalesOrderHeader].[RevisionNumber] AS [RevisionNumber],
    [SalesOrderHeader].[OrderDate] AS [OrderDate],
    [SalesOrderHeader].[DueDate] AS [DueDate],
    [SalesOrderHeader].[ShipDate] AS [ShipDate],
    [SalesOrderHeader].[Status] AS [Status],
    [SalesOrderHeader].[OnlineOrderFlag] AS [OnlineOrderFlag],
    [SalesOrderHeader].[SalesOrderNumber] AS [SalesOrderNumber],
    [SalesOrderHeader].[PurchaseOrderNumber] AS [PurchaseOrderNumber],
    [SalesOrderHeader].[AccountNumber] AS [AccountNumber],
    [SalesOrderHeader].[CustomerID] AS [CustomerID],
    [SalesOrderHeader].[ShipToAddressID] AS [ShipToAddressID],
    [SalesOrderHeader].[BillToAddressID] AS [BillToAddressID],
    [SalesOrderHeader].[ShipMethod] AS [ShipMethod],
    [SalesOrderHeader].[CreditCardApprovalCode] AS [CreditCardApprovalCode],
    [SalesOrderHeader].[SubTotal] AS [SubTotal],
    [SalesOrderHeader].[TaxAmt] AS [TaxAmt],
    [SalesOrderHeader].[Freight] AS [Freight],
    [SalesOrderHeader].[TotalDue] AS [TotalDue],
    [SalesOrderHeader].[Comment] AS [Comment],
    [SalesOrderHeader].[RowGuid] AS [RowGuid],
    [SalesOrderHeader].[ModifiedDate] AS [ModifiedDate]
FROM [SalesLT].[SalesOrderHeader] [SalesOrderHeader]
WHERE (([SalesOrderHeader].[SalesOrderID] = 71774) OR ([SalesOrderHeader].[SalesOrderID] = 71776))
ORDER BY [SalesOrderHeader].[SalesOrderID];
```

## Query Builder

You can build any query via query builder, by calling <xref:DevZest.Data.Primitives.DbSession.CreateQuery*>/<xref:DevZest.Data.Primitives.DbSession.CreateAggregateQuery*> of your `Db` class. You need to provide a delegate to call these methods, which takes a <xref:DevZest.Data.DbQueryBuilder>/<xref:DevZest.Data.DbAggregateQueryBuilder> parameter you can use to build the query. The following code is the equivalent to the preceding example:

```cs
using (var db = new Db(...))
{
    var query = db.CreateQuery((DbQueryBuilder builder) =>
        builder.From(db.SalesOrderHeader, out var x)
            .AutoSelect()
            .Where(x => x.SalesOrderID == _Int32.Const(71774) | x.SalesOrderID == _Int32.Const(71776))
            .OrderBy(x => x.SalesOrderID));
}
```

To use <xref:DevZest.Data.DbQueryBuilder>/<xref:DevZest.Data.DbAggregateQueryBuilder>:

* Call `From` for the first `DbSet<T>`;
* Call `InnerJoin`/`LeftJoin`/`RightJoin`/`CrossJoin` to join other `DbSet<T>`;
* Call `AutoSelect`/`Select` to select columns. `AutoSelect` is based on the <xref:DevZest.Data.Column.Id> and then <xref:DevZest.Data.Column.OriginalId> property value of <xref:DevZest.Data.Column> class;
* Optionally call `Where`/`OrderBy`, or `GroupBy`/`Having` if it's an aggregate query.

Query builder mimics standard SQL SELECT statement closely. It's straightforward if you're familiar with writing SQL. You can find more examples in `Db.Api.cs` (or `Db.Api.vb`) of the `AdventureWorksLT` sample.
