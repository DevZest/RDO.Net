using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Threading.Tasks;

namespace DevZest.Data
{
    [TestClass]
    public class DbSetTests : AdventureWorksTestsBase
    {
        [TestMethod]
        public void DbSet_ToTempTable()
        {
            var log = new StringBuilder();
            using (var db = OpenDb(log))
            {
                var salesOrders = db.SalesOrders.Where(x => x.SalesOrderID == 71774 | x.SalesOrderID == 71776).OrderBy(x => x.SalesOrderID);
                var salesOrderDetails = salesOrders.CreateChild(x => x.SalesOrderDetails, db.SalesOrderDetails);

                var parentTable = salesOrders.ToTempTable();
                var childTable = salesOrderDetails.ToTempTable();
            }
            var expectedSql =
@"CREATE TABLE [#sys_sequential_SalesOrder] (
    [SalesOrderID] INT NOT NULL,
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY NONCLUSTERED ([SalesOrderID]),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);

INSERT INTO [#sys_sequential_SalesOrder]
([SalesOrderID])
SELECT [SalesOrder].[SalesOrderID] AS [SalesOrderID]
FROM [SalesLT].[SalesOrderHeader] [SalesOrder]
WHERE (([SalesOrder].[SalesOrderID] = @p1) OR ([SalesOrder].[SalesOrderID] = @p2))
ORDER BY [SalesOrder].[SalesOrderID];
-- @p1: '71774' (Type = Int32)
-- @p2: '71776' (Type = Int32)

CREATE TABLE [#SalesOrder] (
    [SalesOrderID] INT NOT NULL,
    [RevisionNumber] TINYINT NOT NULL DEFAULT(0),
    [OrderDate] DATETIME NOT NULL DEFAULT(GETDATE()),
    [DueDate] DATETIME NOT NULL,
    [ShipDate] DATETIME NULL,
    [Status] TINYINT NOT NULL DEFAULT(1),
    [OnlineOrderFlag] BIT NOT NULL DEFAULT(1),
    [SalesOrderNumber] NVARCHAR(25) NULL,
    [PurchaseOrderNumber] NVARCHAR(25) NULL,
    [AccountNumber] NVARCHAR(15) NULL,
    [CustomerID] INT NOT NULL,
    [ShipToAddressID] INT NULL,
    [BillToAddressID] INT NULL,
    [ShipMethod] NVARCHAR(50) NOT NULL,
    [CreditCardApprovalCode] NVARCHAR(15) NULL,
    [SubTotal] MONEY NOT NULL DEFAULT(0),
    [TaxAmt] MONEY NOT NULL DEFAULT(0),
    [Freight] MONEY NOT NULL DEFAULT(0),
    [TotalDue] MONEY NOT NULL DEFAULT(0),
    [Comment] NVARCHAR(MAX) NULL,
    [RowGuid] UNIQUEIDENTIFIER NOT NULL DEFAULT(NEWID()),
    [ModifiedDate] DATETIME NOT NULL DEFAULT(GETDATE()),
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY NONCLUSTERED ([SalesOrderID]),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);

INSERT INTO [#SalesOrder]
([SalesOrderID], [RevisionNumber], [OrderDate], [DueDate], [ShipDate], [Status], [OnlineOrderFlag], [SalesOrderNumber], [PurchaseOrderNumber], [AccountNumber], [CustomerID], [ShipToAddressID], [BillToAddressID], [ShipMethod], [CreditCardApprovalCode], [SubTotal], [TaxAmt], [Freight], [TotalDue], [Comment], [RowGuid], [ModifiedDate])
SELECT
    [SalesOrder].[SalesOrderID] AS [SalesOrderID],
    [SalesOrder].[RevisionNumber] AS [RevisionNumber],
    [SalesOrder].[OrderDate] AS [OrderDate],
    [SalesOrder].[DueDate] AS [DueDate],
    [SalesOrder].[ShipDate] AS [ShipDate],
    [SalesOrder].[Status] AS [Status],
    [SalesOrder].[OnlineOrderFlag] AS [OnlineOrderFlag],
    [SalesOrder].[SalesOrderNumber] AS [SalesOrderNumber],
    [SalesOrder].[PurchaseOrderNumber] AS [PurchaseOrderNumber],
    [SalesOrder].[AccountNumber] AS [AccountNumber],
    [SalesOrder].[CustomerID] AS [CustomerID],
    [SalesOrder].[ShipToAddressID] AS [ShipToAddressID],
    [SalesOrder].[BillToAddressID] AS [BillToAddressID],
    [SalesOrder].[ShipMethod] AS [ShipMethod],
    [SalesOrder].[CreditCardApprovalCode] AS [CreditCardApprovalCode],
    [SalesOrder].[SubTotal] AS [SubTotal],
    [SalesOrder].[TaxAmt] AS [TaxAmt],
    [SalesOrder].[Freight] AS [Freight],
    [SalesOrder].[TotalDue] AS [TotalDue],
    [SalesOrder].[Comment] AS [Comment],
    [SalesOrder].[RowGuid] AS [RowGuid],
    [SalesOrder].[ModifiedDate] AS [ModifiedDate]
FROM [SalesLT].[SalesOrderHeader] [SalesOrder]
WHERE (([SalesOrder].[SalesOrderID] = @p1) OR ([SalesOrder].[SalesOrderID] = @p2))
ORDER BY [SalesOrder].[SalesOrderID];
-- @p1: '71774' (Type = Int32)
-- @p2: '71776' (Type = Int32)

CREATE TABLE [#SalesOrderDetail] (
    [SalesOrderID] INT NOT NULL,
    [SalesOrderDetailID] INT NOT NULL,
    [OrderQty] SMALLINT NOT NULL,
    [ProductID] INT NOT NULL,
    [UnitPrice] MONEY NOT NULL,
    [UnitPriceDiscount] MONEY NOT NULL DEFAULT(0),
    [LineTotal] MONEY NOT NULL,
    [RowGuid] UNIQUEIDENTIFIER NOT NULL DEFAULT(NEWID()),
    [ModifiedDate] DATETIME NOT NULL DEFAULT(GETDATE()),
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY NONCLUSTERED ([SalesOrderID], [SalesOrderDetailID]),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);

INSERT INTO [#SalesOrderDetail]
([SalesOrderID], [SalesOrderDetailID], [OrderQty], [ProductID], [UnitPrice], [UnitPriceDiscount], [LineTotal], [RowGuid], [ModifiedDate])
SELECT
    [SalesOrderDetail].[SalesOrderID] AS [SalesOrderID],
    [SalesOrderDetail].[SalesOrderDetailID] AS [SalesOrderDetailID],
    [SalesOrderDetail].[OrderQty] AS [OrderQty],
    [SalesOrderDetail].[ProductID] AS [ProductID],
    [SalesOrderDetail].[UnitPrice] AS [UnitPrice],
    [SalesOrderDetail].[UnitPriceDiscount] AS [UnitPriceDiscount],
    [SalesOrderDetail].[LineTotal] AS [LineTotal],
    [SalesOrderDetail].[RowGuid] AS [RowGuid],
    [SalesOrderDetail].[ModifiedDate] AS [ModifiedDate]
FROM
    ([SalesLT].[SalesOrderDetail] [SalesOrderDetail]
    INNER JOIN
    [#sys_sequential_SalesOrder] [sys_sequential_SalesOrder]
    ON [SalesOrderDetail].[SalesOrderID] = [sys_sequential_SalesOrder].[SalesOrderID])
ORDER BY [sys_sequential_SalesOrder].[sys_row_id] ASC;
";
            Assert.AreEqual(expectedSql.Trim(), log.ToString().Trim());
        }

        [TestMethod]
        public async Task DbSet_ToTempTableAsync()
        {
            var log = new StringBuilder();
            using (var db = await OpenDbAsync(log))
            {
                var salesOrders = db.SalesOrders.Where(x => x.SalesOrderID == 71774 | x.SalesOrderID == 71776).OrderBy(x => x.SalesOrderID);
                var salesOrderDetails = await salesOrders.CreateChildAsync(x => x.SalesOrderDetails, db.SalesOrderDetails);

                var parentTable = await salesOrders.ToTempTableAsync();
                var childTable = await salesOrderDetails.ToTempTableAsync();
            }
            var expectedSql =
@"CREATE TABLE [#sys_sequential_SalesOrder] (
    [SalesOrderID] INT NOT NULL,
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY NONCLUSTERED ([SalesOrderID]),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);

INSERT INTO [#sys_sequential_SalesOrder]
([SalesOrderID])
SELECT [SalesOrder].[SalesOrderID] AS [SalesOrderID]
FROM [SalesLT].[SalesOrderHeader] [SalesOrder]
WHERE (([SalesOrder].[SalesOrderID] = @p1) OR ([SalesOrder].[SalesOrderID] = @p2))
ORDER BY [SalesOrder].[SalesOrderID];
-- @p1: '71774' (Type = Int32)
-- @p2: '71776' (Type = Int32)

CREATE TABLE [#SalesOrder] (
    [SalesOrderID] INT NOT NULL,
    [RevisionNumber] TINYINT NOT NULL DEFAULT(0),
    [OrderDate] DATETIME NOT NULL DEFAULT(GETDATE()),
    [DueDate] DATETIME NOT NULL,
    [ShipDate] DATETIME NULL,
    [Status] TINYINT NOT NULL DEFAULT(1),
    [OnlineOrderFlag] BIT NOT NULL DEFAULT(1),
    [SalesOrderNumber] NVARCHAR(25) NULL,
    [PurchaseOrderNumber] NVARCHAR(25) NULL,
    [AccountNumber] NVARCHAR(15) NULL,
    [CustomerID] INT NOT NULL,
    [ShipToAddressID] INT NULL,
    [BillToAddressID] INT NULL,
    [ShipMethod] NVARCHAR(50) NOT NULL,
    [CreditCardApprovalCode] NVARCHAR(15) NULL,
    [SubTotal] MONEY NOT NULL DEFAULT(0),
    [TaxAmt] MONEY NOT NULL DEFAULT(0),
    [Freight] MONEY NOT NULL DEFAULT(0),
    [TotalDue] MONEY NOT NULL DEFAULT(0),
    [Comment] NVARCHAR(MAX) NULL,
    [RowGuid] UNIQUEIDENTIFIER NOT NULL DEFAULT(NEWID()),
    [ModifiedDate] DATETIME NOT NULL DEFAULT(GETDATE()),
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY NONCLUSTERED ([SalesOrderID]),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);

INSERT INTO [#SalesOrder]
([SalesOrderID], [RevisionNumber], [OrderDate], [DueDate], [ShipDate], [Status], [OnlineOrderFlag], [SalesOrderNumber], [PurchaseOrderNumber], [AccountNumber], [CustomerID], [ShipToAddressID], [BillToAddressID], [ShipMethod], [CreditCardApprovalCode], [SubTotal], [TaxAmt], [Freight], [TotalDue], [Comment], [RowGuid], [ModifiedDate])
SELECT
    [SalesOrder].[SalesOrderID] AS [SalesOrderID],
    [SalesOrder].[RevisionNumber] AS [RevisionNumber],
    [SalesOrder].[OrderDate] AS [OrderDate],
    [SalesOrder].[DueDate] AS [DueDate],
    [SalesOrder].[ShipDate] AS [ShipDate],
    [SalesOrder].[Status] AS [Status],
    [SalesOrder].[OnlineOrderFlag] AS [OnlineOrderFlag],
    [SalesOrder].[SalesOrderNumber] AS [SalesOrderNumber],
    [SalesOrder].[PurchaseOrderNumber] AS [PurchaseOrderNumber],
    [SalesOrder].[AccountNumber] AS [AccountNumber],
    [SalesOrder].[CustomerID] AS [CustomerID],
    [SalesOrder].[ShipToAddressID] AS [ShipToAddressID],
    [SalesOrder].[BillToAddressID] AS [BillToAddressID],
    [SalesOrder].[ShipMethod] AS [ShipMethod],
    [SalesOrder].[CreditCardApprovalCode] AS [CreditCardApprovalCode],
    [SalesOrder].[SubTotal] AS [SubTotal],
    [SalesOrder].[TaxAmt] AS [TaxAmt],
    [SalesOrder].[Freight] AS [Freight],
    [SalesOrder].[TotalDue] AS [TotalDue],
    [SalesOrder].[Comment] AS [Comment],
    [SalesOrder].[RowGuid] AS [RowGuid],
    [SalesOrder].[ModifiedDate] AS [ModifiedDate]
FROM [SalesLT].[SalesOrderHeader] [SalesOrder]
WHERE (([SalesOrder].[SalesOrderID] = @p1) OR ([SalesOrder].[SalesOrderID] = @p2))
ORDER BY [SalesOrder].[SalesOrderID];
-- @p1: '71774' (Type = Int32)
-- @p2: '71776' (Type = Int32)

CREATE TABLE [#SalesOrderDetail] (
    [SalesOrderID] INT NOT NULL,
    [SalesOrderDetailID] INT NOT NULL,
    [OrderQty] SMALLINT NOT NULL,
    [ProductID] INT NOT NULL,
    [UnitPrice] MONEY NOT NULL,
    [UnitPriceDiscount] MONEY NOT NULL DEFAULT(0),
    [LineTotal] MONEY NOT NULL,
    [RowGuid] UNIQUEIDENTIFIER NOT NULL DEFAULT(NEWID()),
    [ModifiedDate] DATETIME NOT NULL DEFAULT(GETDATE()),
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY NONCLUSTERED ([SalesOrderID], [SalesOrderDetailID]),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);

INSERT INTO [#SalesOrderDetail]
([SalesOrderID], [SalesOrderDetailID], [OrderQty], [ProductID], [UnitPrice], [UnitPriceDiscount], [LineTotal], [RowGuid], [ModifiedDate])
SELECT
    [SalesOrderDetail].[SalesOrderID] AS [SalesOrderID],
    [SalesOrderDetail].[SalesOrderDetailID] AS [SalesOrderDetailID],
    [SalesOrderDetail].[OrderQty] AS [OrderQty],
    [SalesOrderDetail].[ProductID] AS [ProductID],
    [SalesOrderDetail].[UnitPrice] AS [UnitPrice],
    [SalesOrderDetail].[UnitPriceDiscount] AS [UnitPriceDiscount],
    [SalesOrderDetail].[LineTotal] AS [LineTotal],
    [SalesOrderDetail].[RowGuid] AS [RowGuid],
    [SalesOrderDetail].[ModifiedDate] AS [ModifiedDate]
FROM
    ([SalesLT].[SalesOrderDetail] [SalesOrderDetail]
    INNER JOIN
    [#sys_sequential_SalesOrder] [sys_sequential_SalesOrder]
    ON [SalesOrderDetail].[SalesOrderID] = [sys_sequential_SalesOrder].[SalesOrderID])
ORDER BY [sys_sequential_SalesOrder].[sys_row_id] ASC;
";
            Assert.AreEqual(expectedSql.Trim(), log.ToString().Trim());
        }
    }
}