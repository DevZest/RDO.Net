using AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Threading.Tasks;

namespace DevZest.Data
{
    [TestClass]
    public class DbSessionTests : AdventureWorksTestsBase
    {
        private static DbQuery<SalesOrder> CreateSalesOrdersQuery(Db db)
        {
            return db.CreateQuery((DbQueryBuilder builder, SalesOrder model) =>
            {
                SalesOrder h;
                builder.From(db.SalesOrders, out h)
                    .AutoSelect()
                    .Where(h.SalesOrderID == _Int32.Const(71774) | h.SalesOrderID == _Int32.Const(71776))
                    .OrderBy(h.SalesOrderID);
            });
        }

        private static void GetSalesOrderDetails(Db db, DbQueryBuilder queryBuilder, SalesOrderDetail model)
        {
            SalesOrderDetail d;
            queryBuilder.From(db.SalesOrderDetails, out d)
                .AutoSelect()
                .OrderBy(d.SalesOrderDetailID);
        }

        [TestMethod]
        public void DbSession_CreateTempTable()
        {
            var log = new StringBuilder();
            using (var db = OpenDb(log))
            {
                var salesOrders = db.CreateTempTable((DbQueryBuilder builder, SalesOrder model) =>
                {
                    SalesOrder h;
                    builder.From(db.SalesOrders, out h)
                        .AutoSelect()
                        .Where(h.SalesOrderID == 71774 | h.SalesOrderID == 71776)
                        .OrderBy(h.SalesOrderID);
                });
                var salesOrderDetails = salesOrders.CreateChild(x => x.SalesOrderDetails, (DbQueryBuilder builder, SalesOrderDetail model) => GetSalesOrderDetails(db, builder, model));

                Assert.AreEqual(2, salesOrders.GetInitialRowCount());
                Assert.AreEqual(3, salesOrderDetails.GetInitialRowCount());
            }
            var expectedSql =
@"CREATE TABLE [#SalesOrder] (
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
    [#SalesOrder] [SalesOrder]
    ON [SalesOrderDetail].[SalesOrderID] = [SalesOrder].[SalesOrderID])
ORDER BY [SalesOrder].[sys_row_id] ASC, [SalesOrderDetail].[SalesOrderDetailID];
";
            Assert.AreEqual(expectedSql.Trim(), log.ToString().Trim());
        }

        [TestMethod]
        public async Task DbSession_CreateTempTableAsync()
        {
            var log = new StringBuilder();
            using (var db = await OpenDbAsync(log))
            {
                var salesOrders = await db.CreateTempTableAsync((DbQueryBuilder builder, SalesOrder model) =>
                {
                    SalesOrder h;
                    builder.From(db.SalesOrders, out h)
                        .AutoSelect()
                        .Where(h.SalesOrderID == 71774 | h.SalesOrderID == 71776)
                        .OrderBy(h.SalesOrderID);
                });
                var salesOrderDetails = await salesOrders.CreateChildAsync(x => x.SalesOrderDetails, (DbQueryBuilder builder, SalesOrderDetail model) => GetSalesOrderDetails(db, builder, model));

                Assert.AreEqual(2, await salesOrders.GetInitialRowCountAsync());
                Assert.AreEqual(3, await salesOrderDetails.GetInitialRowCountAsync());
            }
            var expectedSql =
@"CREATE TABLE [#SalesOrder] (
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
    [#SalesOrder] [SalesOrder]
    ON [SalesOrderDetail].[SalesOrderID] = [SalesOrder].[SalesOrderID])
ORDER BY [SalesOrder].[sys_row_id] ASC, [SalesOrderDetail].[SalesOrderDetailID];
";
            Assert.AreEqual(expectedSql.Trim(), log.ToString().Trim());
        }

        [TestMethod]
        public void DbSession_CreateQuery()
        {
            var log = new StringBuilder();
            using (var db = OpenDb(log))
            {
                var salesOrders = CreateSalesOrdersQuery(db);
                var salesOrderDetails = salesOrders.CreateChild(x => x.SalesOrderDetails, (DbQueryBuilder builder, SalesOrderDetail model) => GetSalesOrderDetails(db, builder, model));

                Assert.AreEqual(2, salesOrders.GetInitialRowCount());
                Assert.AreEqual(3, salesOrderDetails.GetInitialRowCount());
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
WHERE (([SalesOrder].[SalesOrderID] = 71774) OR ([SalesOrder].[SalesOrderID] = 71776))
ORDER BY [SalesOrder].[SalesOrderID];

CREATE TABLE [#sys_sequential_SalesOrderDetail] (
    [SalesOrderID] INT NOT NULL,
    [SalesOrderDetailID] INT NOT NULL,
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY NONCLUSTERED ([SalesOrderID], [SalesOrderDetailID]),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);

INSERT INTO [#sys_sequential_SalesOrderDetail]
([SalesOrderID], [SalesOrderDetailID])
SELECT
    [SalesOrderDetail].[SalesOrderID] AS [SalesOrderID],
    [SalesOrderDetail].[SalesOrderDetailID] AS [SalesOrderDetailID]
FROM
    ([SalesLT].[SalesOrderDetail] [SalesOrderDetail]
    INNER JOIN
    [#sys_sequential_SalesOrder] [sys_sequential_SalesOrder]
    ON [SalesOrderDetail].[SalesOrderID] = [sys_sequential_SalesOrder].[SalesOrderID])
ORDER BY [sys_sequential_SalesOrder].[sys_row_id] ASC, [SalesOrderDetail].[SalesOrderDetailID];
";
            Assert.AreEqual(expectedSql.Trim(), log.ToString().Trim());
        }

        [TestMethod]
        public async Task DbSession_CreateQuery_async_child()
        {
            var log = new StringBuilder();
            using (var db = await OpenDbAsync(log))
            {
                var salesOrders = CreateSalesOrdersQuery(db);
                var salesOrderDetails = await salesOrders.CreateChildAsync(x => x.SalesOrderDetails,
                    (DbQueryBuilder builder, SalesOrderDetail model) => GetSalesOrderDetails(db, builder, model));

                Assert.AreEqual(2, await salesOrders.GetInitialRowCountAsync());
                Assert.AreEqual(3, await salesOrderDetails.GetInitialRowCountAsync());
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
WHERE (([SalesOrder].[SalesOrderID] = 71774) OR ([SalesOrder].[SalesOrderID] = 71776))
ORDER BY [SalesOrder].[SalesOrderID];

CREATE TABLE [#sys_sequential_SalesOrderDetail] (
    [SalesOrderID] INT NOT NULL,
    [SalesOrderDetailID] INT NOT NULL,
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY NONCLUSTERED ([SalesOrderID], [SalesOrderDetailID]),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);

INSERT INTO [#sys_sequential_SalesOrderDetail]
([SalesOrderID], [SalesOrderDetailID])
SELECT
    [SalesOrderDetail].[SalesOrderID] AS [SalesOrderID],
    [SalesOrderDetail].[SalesOrderDetailID] AS [SalesOrderDetailID]
FROM
    ([SalesLT].[SalesOrderDetail] [SalesOrderDetail]
    INNER JOIN
    [#sys_sequential_SalesOrder] [sys_sequential_SalesOrder]
    ON [SalesOrderDetail].[SalesOrderID] = [sys_sequential_SalesOrder].[SalesOrderID])
ORDER BY [sys_sequential_SalesOrder].[sys_row_id] ASC, [SalesOrderDetail].[SalesOrderDetailID];
";
            Assert.AreEqual(expectedSql.Trim(), log.ToString().Trim());
        }

        private static void GetDistinctSalesOrderDetails(Db db, DbAggregateQueryBuilder queryBuilder, SalesOrderDetail model)
        {
            SalesOrderDetail d;
            queryBuilder.From(db.SalesOrderDetails, out d)
                .AutoSelect()
                .OrderBy(d.SalesOrderDetailID);
        }

        [TestMethod]
        public void DbSession_CreateQuery_aggregate_child()
        {
            var log = new StringBuilder();
            using (var db = OpenDb(log))
            {
                var salesOrders = CreateSalesOrdersQuery(db);
                var salesOrderDetails = salesOrders.CreateChild(x => x.SalesOrderDetails, (DbAggregateQueryBuilder builder, SalesOrderDetail model) => GetDistinctSalesOrderDetails(db, builder, model));

                Assert.AreEqual(2, salesOrders.GetInitialRowCount());
                Assert.AreEqual(3, salesOrderDetails.GetInitialRowCount());
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
WHERE (([SalesOrder].[SalesOrderID] = 71774) OR ([SalesOrder].[SalesOrderID] = 71776))
ORDER BY [SalesOrder].[SalesOrderID];

CREATE TABLE [#sys_sequential_SalesOrderDetail] (
    [SalesOrderID] INT NOT NULL,
    [SalesOrderDetailID] INT NOT NULL,
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY NONCLUSTERED ([SalesOrderID], [SalesOrderDetailID]),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);

INSERT INTO [#sys_sequential_SalesOrderDetail]
([SalesOrderID], [SalesOrderDetailID])
SELECT
    [SalesOrderDetail].[SalesOrderID] AS [SalesOrderID],
    [SalesOrderDetail].[SalesOrderDetailID] AS [SalesOrderDetailID]
FROM
    ([SalesLT].[SalesOrderDetail] [SalesOrderDetail]
    INNER JOIN
    [#sys_sequential_SalesOrder] [sys_sequential_SalesOrder]
    ON [SalesOrderDetail].[SalesOrderID] = [sys_sequential_SalesOrder].[SalesOrderID])
GROUP BY
    [SalesOrderDetail].[SalesOrderID],
    [SalesOrderDetail].[SalesOrderDetailID],
    [SalesOrderDetail].[OrderQty],
    [SalesOrderDetail].[ProductID],
    [SalesOrderDetail].[UnitPrice],
    [SalesOrderDetail].[UnitPriceDiscount],
    [SalesOrderDetail].[LineTotal],
    [SalesOrderDetail].[RowGuid],
    [SalesOrderDetail].[ModifiedDate],
    [sys_sequential_SalesOrder].[sys_row_id]
ORDER BY [sys_sequential_SalesOrder].[sys_row_id] ASC, [SalesOrderDetail].[SalesOrderDetailID];
";
            Assert.AreEqual(expectedSql.Trim(), log.ToString().Trim());
        }

        [TestMethod]
        public void DbSession_ExecuteReader()
        {
            var log = new StringBuilder();
            using (var db = OpenDb(log))
            {
                var customers = db.Customers.OrderBy(x => x.CustomerID);
                var c = customers._;
                using (var reader = db.ExecuteReader(customers))
                {
                    reader.Read();
                    var id = c.CustomerID[reader];
                    Assert.AreEqual(1, id);
                }
            }
            var expectedSql =
@"SELECT
    [Customer].[CustomerID] AS [CustomerID],
    [Customer].[NameStyle] AS [NameStyle],
    [Customer].[Title] AS [Title],
    [Customer].[FirstName] AS [FirstName],
    [Customer].[MiddleName] AS [MiddleName],
    [Customer].[LastName] AS [LastName],
    [Customer].[Suffix] AS [Suffix],
    [Customer].[CompanyName] AS [CompanyName],
    [Customer].[SalesPerson] AS [SalesPerson],
    [Customer].[EmailAddress] AS [EmailAddress],
    [Customer].[Phone] AS [Phone],
    [Customer].[PasswordHash] AS [PasswordHash],
    [Customer].[PasswordSalt] AS [PasswordSalt],
    [Customer].[RowGuid] AS [RowGuid],
    [Customer].[ModifiedDate] AS [ModifiedDate]
FROM [SalesLT].[Customer] [Customer]
ORDER BY [Customer].[CustomerID];
";
            Assert.AreEqual(expectedSql.Trim(), log.ToString().Trim());
        }

        [TestMethod]
        public async Task DbSession_ExecuteReaderAsync()
        {
            var log = new StringBuilder();
            using (var db = await OpenDbAsync(log))
            {
                var customers = db.Customers.OrderBy(x => x.CustomerID);
                var c = customers._;
                using (var reader = await db.ExecuteReaderAsync(customers))
                {
                    await reader.ReadAsync();
                    var id = c.CustomerID[reader];
                    Assert.AreEqual(1, id);
                }
            }
            var expectedSql =
@"SELECT
    [Customer].[CustomerID] AS [CustomerID],
    [Customer].[NameStyle] AS [NameStyle],
    [Customer].[Title] AS [Title],
    [Customer].[FirstName] AS [FirstName],
    [Customer].[MiddleName] AS [MiddleName],
    [Customer].[LastName] AS [LastName],
    [Customer].[Suffix] AS [Suffix],
    [Customer].[CompanyName] AS [CompanyName],
    [Customer].[SalesPerson] AS [SalesPerson],
    [Customer].[EmailAddress] AS [EmailAddress],
    [Customer].[Phone] AS [Phone],
    [Customer].[PasswordHash] AS [PasswordHash],
    [Customer].[PasswordSalt] AS [PasswordSalt],
    [Customer].[RowGuid] AS [RowGuid],
    [Customer].[ModifiedDate] AS [ModifiedDate]
FROM [SalesLT].[Customer] [Customer]
ORDER BY [Customer].[CustomerID];
";
            Assert.AreEqual(expectedSql.Trim(), log.ToString().Trim());
        }
    }
}
