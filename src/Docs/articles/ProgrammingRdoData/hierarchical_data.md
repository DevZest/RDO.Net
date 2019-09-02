# Hierarchical Data

Hierarchical data is supported extensively in RDO.Data. You can define child model and RDO.Data will enforce parent-child relationship for <xref:DevZest.Data.DataSet`1>, <xref:DevZest.Data.DbQuery`1> and temporary <xref:DevZest.Data.DbTable`1>.

## Child Model

You can define child model for your model as readonly property as described in <xref:model_and_members>:

# [C#](#tab/cs)

```cs
public abstract class SalesOrderBase<T> : ...
{
    static SalesOrderBase()
    {
        ...
        RegisterChildModel((SalesOrderBase<T> _) => _.SalesOrderDetails, (T _) => _.FK_SalesOrderHeader);
    }

    ...
    public T SalesOrderDetails { get; private set; }
}
```

# [VB.Net](#tab/vb)

```vb
Public MustInherit Class SalesOrderBase(Of T As {SalesOrderDetail, New})
    ...
    Shared Sub New()
        ...
        RegisterChildModel(Function(x As SalesOrderBase(Of T)) x.SalesOrderDetails, Function(x As T) x.FK_SalesOrderHeader)
    End Sub

    ...
    Private m_SalesOrderDetails As T
    Public Property SalesOrderDetails As T
        Get
            Return m_SalesOrderDetails
        End Get
        Private Set
            m_SalesOrderDetails = Value
        End Set
    End Property
End Class
```

***

You can use Model Visualizer tool window to add child model and corresponding registration conveniently, similar as model column. The foreign key relationship defined in your child model will be picked up automatically.

>[!Note]
>For simplicity, generic type child model, as shown in preceding example, has no code-fix registration supported. You can workaround this limitation by temporarily change the child model property type to concrete type.

## Child DataSet

If a model contains a child model, the corresponding DataSet will contain a child DataSet. Each <xref:DevZest.Data.DataRow> of child DataSet is belonging to two DataSets: the base DataSet identified by its <xref:DevZest.Data.DataRow.BaseDataSet> and <xref:DevZest.Data.DataRow.Ordinal> property, and child DataSet identified by its <xref:DevZest.Data.DataRow.DataSet> and <xref:DevZest.Data.DataRow.Index> property.

Child Model/DataSet is governed by its parent Model/DataSet:

* The column of the child model participated in the parent-child relationship is readonly. The data values of this column are inherited from the value of its parent DataRow.
* Deleting the parent DataRow will delete the child DataSet contained by this row automatically.

The following table shows APIs related to child DataSet:

| API | Description |
|-----|-------------|
| [DataSet\<T\>.GetChild](xref:DevZest.Data.DataSet`1.GetChild*) | Get child DataSet from parent DataSet. |
| [DataSet\<T\>.FillChildAsync](xref:DevZest.Data.DataSet`1.GetChild*) | Fill data values into child DataSet. |
| <xref:DevZest.Data.Extensions.GetChildDataSet*> extension method | Get Child DataSet from child model object. |

## Child Query/Temp Table

You can call [DbQuery\<T\>.CreateChildAsync](xref:DevZest.Data.DbQuery`1.CreateChildAsync*) to create child query:

# [C#](#tab/cs)

```cs
public async Task<DataSet<SalesOrderInfo>> GetSalesOrderInfoAsync(_Int32 salesOrderID, CancellationToken ct = default(CancellationToken))
{
    var result = CreateQuery((DbQueryBuilder builder, SalesOrderInfo _) =>
    {
        builder.From(SalesOrderHeader, out var o)
            .LeftJoin(Customer, o.FK_Customer, out var c)
            .LeftJoin(Address, o.FK_ShipToAddress, out var shipTo)
            .LeftJoin(Address, o.FK_BillToAddress, out var billTo)
            .AutoSelect()
            .AutoSelect(c, _.Customer)
            .AutoSelect(shipTo, _.ShipToAddress)
            .AutoSelect(billTo, _.BillToAddress)
            .Where(o.SalesOrderID == salesOrderID);
    });

    await result.CreateChildAsync(_ => _.SalesOrderDetails, (DbQueryBuilder builder, SalesOrderInfoDetail _) =>
    {
        builder.From(SalesOrderDetail, out var d)
            .LeftJoin(Product, d.FK_Product, out var p)
            .AutoSelect()
            .AutoSelect(p, _.Product)
            .OrderBy(d.SalesOrderDetailID);
    }, ct);

    return await result.ToDataSetAsync(ct);
}
```
# [VB.Net](#tab/vb)

```vb
Public Async Function GetSalesOrderInfoAsync(salesOrderID As _Int32, Optional ct As CancellationToken = Nothing) As Task(Of DataSet(Of SalesOrderInfo))
    Dim result = CreateQuery(
        Sub(builder As DbQueryBuilder, x As SalesOrderInfo)
            Dim o As SalesOrderHeader = Nothing, c As Customer = Nothing, shipTo As Address = Nothing, billTo As Address = Nothing
            builder.From(SalesOrderHeader, o).
                LeftJoin(Customer, o.FK_Customer, c).
                LeftJoin(Address, o.FK_ShipToAddress, shipTo).
                LeftJoin(Address, o.FK_BillToAddress, billTo).
                AutoSelect().
                AutoSelect(c, x.Customer).
                AutoSelect(shipTo, x.ShipToAddress).
                AutoSelect(billTo, x.BillToAddress).
                Where(o.SalesOrderID = salesOrderID)
        End Sub)
    Await result.CreateChildAsync(Function(x) x.SalesOrderDetails,
        Sub(builder As DbQueryBuilder, x As SalesOrderInfoDetail)
            Dim d As SalesOrderDetail = Nothing, p As Product = Nothing
            builder.From(SalesOrderDetail, d).LeftJoin(Product, d.FK_Product, p).AutoSelect().AutoSelect(p, x.Product)
        End Sub, ct)
    Return Await result.ToDataSetAsync(ct)
End Function
```

***

RDO.Data will automatically create a temp table which contains the primary key of the parent query, and join this temp table automatically with child query. The above query will be translated to the following SQL in SQL Server:

```sql
CREATE TABLE [#sys_sequential_SalesOrderInfo] (
    [SalesOrderID] INT NOT NULL,
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY NONCLUSTERED ([SalesOrderID]),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);

INSERT INTO [#sys_sequential_SalesOrderInfo]
([SalesOrderID])
SELECT [SalesOrderHeader].[SalesOrderID] AS [SalesOrderID]
FROM
    ((([SalesLT].[SalesOrderHeader] [SalesOrderHeader]
    LEFT JOIN
    [SalesLT].[Customer] [Customer]
    ON [SalesOrderHeader].[CustomerID] = [Customer].[CustomerID])
    LEFT JOIN
    [SalesLT].[Address] [Address]
    ON [SalesOrderHeader].[ShipToAddressID] = [Address].[AddressID])
    LEFT JOIN
    [SalesLT].[Address] [Address1]
    ON [SalesOrderHeader].[BillToAddressID] = [Address1].[AddressID])
WHERE ([SalesOrderHeader].[SalesOrderID] = @p1);
-- @p1: '71774' (Type = Int32)

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
    [SalesOrderHeader].[ModifiedDate] AS [ModifiedDate],
    NULL AS [LineCount],
    [Customer].[Title] AS [Customer.Title],
    [Customer].[FirstName] AS [Customer.FirstName],
    [Customer].[MiddleName] AS [Customer.MiddleName],
    [Customer].[LastName] AS [Customer.LastName],
    [Customer].[CompanyName] AS [Customer.CompanyName],
    [Customer].[EmailAddress] AS [Customer.EmailAddress],
    [Customer].[Phone] AS [Customer.Phone],
    [Address].[AddressLine1] AS [ShipToAddress.AddressLine1],
    [Address].[AddressLine2] AS [ShipToAddress.AddressLine2],
    [Address].[City] AS [ShipToAddress.City],
    [Address].[StateProvince] AS [ShipToAddress.StateProvince],
    [Address].[CountryRegion] AS [ShipToAddress.CountryRegion],
    [Address].[PostalCode] AS [ShipToAddress.PostalCode],
    [Address1].[AddressLine1] AS [BillToAddress.AddressLine1],
    [Address1].[AddressLine2] AS [BillToAddress.AddressLine2],
    [Address1].[City] AS [BillToAddress.City],
    [Address1].[StateProvince] AS [BillToAddress.StateProvince],
    [Address1].[CountryRegion] AS [BillToAddress.CountryRegion],
    [Address1].[PostalCode] AS [BillToAddress.PostalCode],
    [#sys_sequential_SalesOrderInfo].[sys_row_id] AS [sys_row_id]
FROM
    (((([SalesLT].[SalesOrderHeader] [SalesOrderHeader]
    LEFT JOIN
    [SalesLT].[Customer] [Customer]
    ON [SalesOrderHeader].[CustomerID] = [Customer].[CustomerID])
    LEFT JOIN
    [SalesLT].[Address] [Address]
    ON [SalesOrderHeader].[ShipToAddressID] = [Address].[AddressID])
    LEFT JOIN
    [SalesLT].[Address] [Address1]
    ON [SalesOrderHeader].[BillToAddressID] = [Address1].[AddressID])
    INNER JOIN
    [#sys_sequential_SalesOrderInfo]
    ON [SalesOrderHeader].[SalesOrderID] = [#sys_sequential_SalesOrderInfo].[SalesOrderID])
WHERE ([SalesOrderHeader].[SalesOrderID] = @p1)
ORDER BY [#sys_sequential_SalesOrderInfo].[sys_row_id] ASC;
-- @p1: '71774' (Type = Int32)

SELECT
    [SalesOrderDetail].[SalesOrderID] AS [SalesOrderID],
    [SalesOrderDetail].[SalesOrderDetailID] AS [SalesOrderDetailID],
    [SalesOrderDetail].[OrderQty] AS [OrderQty],
    [SalesOrderDetail].[ProductID] AS [ProductID],
    [SalesOrderDetail].[UnitPrice] AS [UnitPrice],
    [SalesOrderDetail].[UnitPriceDiscount] AS [UnitPriceDiscount],
    [SalesOrderDetail].[LineTotal] AS [LineTotal],
    [SalesOrderDetail].[RowGuid] AS [RowGuid],
    [SalesOrderDetail].[ModifiedDate] AS [ModifiedDate],
    [Product].[Name] AS [Product.Name],
    [Product].[ProductNumber] AS [Product.ProductNumber],
    [#sys_sequential_SalesOrderInfo].[sys_row_id] AS [sys_parent_row_id]
FROM
    (([SalesLT].[SalesOrderDetail] [SalesOrderDetail]
    LEFT JOIN
    [SalesLT].[Product] [Product]
    ON [SalesOrderDetail].[ProductID] = [Product].[ProductID])
    INNER JOIN
    [#sys_sequential_SalesOrderInfo]
    ON [SalesOrderDetail].[SalesOrderID] = [#sys_sequential_SalesOrderInfo].[SalesOrderID])
ORDER BY [#sys_sequential_SalesOrderInfo].[sys_row_id] ASC, [SalesOrderDetail].[SalesOrderDetailID];
```

You can also call [DbTable\<T\>.CreateChildAsync](xref:DevZest.Data.DbTable`1.CreateChildAsync*) to create child temporary table, similar with child query.
