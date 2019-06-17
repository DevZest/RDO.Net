using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.MySql
{
    [TestClass]
    public class DbSetTests
    {
        [TestMethod]
        public void DbSet_Scalar()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var query = db.CreateQuery<SalesOrderHeader.Key>((builder, _) => builder.Select(_Int32.Const(1), _.SalesOrderID));
                var expectedSql =
@"SELECT 1 AS `SalesOrderID`;
";
                Assert.AreEqual(expectedSql, query.ToString());
            }
        }

        [TestMethod]
        public void DbSet_Scalar_LeftJoin()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var scalar = db.CreateQuery<SalesOrderHeader.Key>((builder, _) => builder.Select(_Int32.Const(1), _.SalesOrderID));
                var query = db.CreateQuery<SalesOrderHeader.Key>((builder, _) =>
                {
                    builder.From(scalar, out var s)
                    .LeftJoin(db.SalesOrderHeader, s.PrimaryKey, out var h)
                    .Select(s.SalesOrderID, _.SalesOrderID)
                    .Where(h.SalesOrderID.IsNotNull());
                });
                var expectedSql =
@"SELECT `ContainerModel`.`SalesOrderID` AS `SalesOrderID`
FROM
    ((SELECT 1 AS `SalesOrderID`) `ContainerModel`
    LEFT JOIN
    `SalesOrderHeader`
    ON `ContainerModel`.`SalesOrderID` = `SalesOrderHeader`.`SalesOrderID`)
WHERE (`SalesOrderHeader`.`SalesOrderID` IS NOT NULL);
";
                Assert.AreEqual(expectedSql, query.ToString());
            }
        }

        [TestMethod]
        public void DbSet_where_order_by()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var query = db.SalesOrderHeader.Where(x => x.SalesOrderID == _Int32.Const(71774) | x.SalesOrderID == _Int32.Const(71776)).OrderBy(x => x.SalesOrderID);
                var expectedSql =
@"SELECT
    `SalesOrderHeader`.`SalesOrderID` AS `SalesOrderID`,
    `SalesOrderHeader`.`RevisionNumber` AS `RevisionNumber`,
    `SalesOrderHeader`.`OrderDate` AS `OrderDate`,
    `SalesOrderHeader`.`DueDate` AS `DueDate`,
    `SalesOrderHeader`.`ShipDate` AS `ShipDate`,
    `SalesOrderHeader`.`Status` AS `Status`,
    `SalesOrderHeader`.`OnlineOrderFlag` AS `OnlineOrderFlag`,
    `SalesOrderHeader`.`SalesOrderNumber` AS `SalesOrderNumber`,
    `SalesOrderHeader`.`PurchaseOrderNumber` AS `PurchaseOrderNumber`,
    `SalesOrderHeader`.`AccountNumber` AS `AccountNumber`,
    `SalesOrderHeader`.`CustomerID` AS `CustomerID`,
    `SalesOrderHeader`.`ShipToAddressID` AS `ShipToAddressID`,
    `SalesOrderHeader`.`BillToAddressID` AS `BillToAddressID`,
    `SalesOrderHeader`.`ShipMethod` AS `ShipMethod`,
    `SalesOrderHeader`.`CreditCardApprovalCode` AS `CreditCardApprovalCode`,
    `SalesOrderHeader`.`SubTotal` AS `SubTotal`,
    `SalesOrderHeader`.`TaxAmt` AS `TaxAmt`,
    `SalesOrderHeader`.`Freight` AS `Freight`,
    `SalesOrderHeader`.`TotalDue` AS `TotalDue`,
    `SalesOrderHeader`.`Comment` AS `Comment`,
    `SalesOrderHeader`.`RowGuid` AS `RowGuid`,
    `SalesOrderHeader`.`ModifiedDate` AS `ModifiedDate`
FROM `SalesOrderHeader`
WHERE ((`SalesOrderHeader`.`SalesOrderID` = 71774) OR (`SalesOrderHeader`.`SalesOrderID` = 71776))
ORDER BY `SalesOrderHeader`.`SalesOrderID`;
";
                Assert.AreEqual(expectedSql, query.ToString());
            }
        }

        [TestMethod]
        public void DbSet_Where_multi_level()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var query = db.ProductCategory.Where(x => x.ParentProductCategoryID.IsNull()).Where(x => x.Name.IsNotNull());
                var expectedSql =
@"SELECT
    `ProductCategory`.`ProductCategoryID` AS `ProductCategoryID`,
    `ProductCategory`.`ParentProductCategoryID` AS `ParentProductCategoryID`,
    `ProductCategory`.`Name` AS `Name`,
    `ProductCategory`.`RowGuid` AS `RowGuid`,
    `ProductCategory`.`ModifiedDate` AS `ModifiedDate`
FROM `ProductCategory`
WHERE ((`ProductCategory`.`ParentProductCategoryID` IS NULL) AND (`ProductCategory`.`Name` IS NOT NULL));
";
                Assert.AreEqual(expectedSql, query.ToString());
            }
        }

        [TestMethod]
        public void DbSet_Union()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var query = db.Product.Where(x => x.ProductID < _Int32.Const(720)).UnionAll(db.Product.Where(x => x.ProductID > _Int32.Const(800)));
                var expectedSql =
@"(SELECT
    `Product`.`ProductID` AS `ProductID`,
    `Product`.`Name` AS `Name`,
    `Product`.`ProductNumber` AS `ProductNumber`,
    `Product`.`Color` AS `Color`,
    `Product`.`StandardCost` AS `StandardCost`,
    `Product`.`ListPrice` AS `ListPrice`,
    `Product`.`Size` AS `Size`,
    `Product`.`Weight` AS `Weight`,
    `Product`.`ProductCategoryID` AS `ProductCategoryID`,
    `Product`.`ProductModelID` AS `ProductModelID`,
    `Product`.`SellStartDate` AS `SellStartDate`,
    `Product`.`SellEndDate` AS `SellEndDate`,
    `Product`.`DiscontinuedDate` AS `DiscontinuedDate`,
    `Product`.`ThumbNailPhoto` AS `ThumbNailPhoto`,
    `Product`.`ThumbnailPhotoFileName` AS `ThumbnailPhotoFileName`,
    `Product`.`RowGuid` AS `RowGuid`,
    `Product`.`ModifiedDate` AS `ModifiedDate`
FROM `Product`
WHERE (`Product`.`ProductID` < 720))
UNION ALL
(SELECT
    `Product`.`ProductID` AS `ProductID`,
    `Product`.`Name` AS `Name`,
    `Product`.`ProductNumber` AS `ProductNumber`,
    `Product`.`Color` AS `Color`,
    `Product`.`StandardCost` AS `StandardCost`,
    `Product`.`ListPrice` AS `ListPrice`,
    `Product`.`Size` AS `Size`,
    `Product`.`Weight` AS `Weight`,
    `Product`.`ProductCategoryID` AS `ProductCategoryID`,
    `Product`.`ProductModelID` AS `ProductModelID`,
    `Product`.`SellStartDate` AS `SellStartDate`,
    `Product`.`SellEndDate` AS `SellEndDate`,
    `Product`.`DiscontinuedDate` AS `DiscontinuedDate`,
    `Product`.`ThumbNailPhoto` AS `ThumbNailPhoto`,
    `Product`.`ThumbnailPhotoFileName` AS `ThumbnailPhotoFileName`,
    `Product`.`RowGuid` AS `RowGuid`,
    `Product`.`ModifiedDate` AS `ModifiedDate`
FROM `Product`
WHERE (`Product`.`ProductID` > 800));
";
                Assert.AreEqual(expectedSql, query.ToString());

                var query2 = query.OrderBy(x => x.Name.Asc());
                expectedSql =
@"SELECT
    `Product`.`ProductID` AS `ProductID`,
    `Product`.`Name` AS `Name`,
    `Product`.`ProductNumber` AS `ProductNumber`,
    `Product`.`Color` AS `Color`,
    `Product`.`StandardCost` AS `StandardCost`,
    `Product`.`ListPrice` AS `ListPrice`,
    `Product`.`Size` AS `Size`,
    `Product`.`Weight` AS `Weight`,
    `Product`.`ProductCategoryID` AS `ProductCategoryID`,
    `Product`.`ProductModelID` AS `ProductModelID`,
    `Product`.`SellStartDate` AS `SellStartDate`,
    `Product`.`SellEndDate` AS `SellEndDate`,
    `Product`.`DiscontinuedDate` AS `DiscontinuedDate`,
    `Product`.`ThumbNailPhoto` AS `ThumbNailPhoto`,
    `Product`.`ThumbnailPhotoFileName` AS `ThumbnailPhotoFileName`,
    `Product`.`RowGuid` AS `RowGuid`,
    `Product`.`ModifiedDate` AS `ModifiedDate`
FROM
    ((SELECT
        `Product`.`ProductID` AS `ProductID`,
        `Product`.`Name` AS `Name`,
        `Product`.`ProductNumber` AS `ProductNumber`,
        `Product`.`Color` AS `Color`,
        `Product`.`StandardCost` AS `StandardCost`,
        `Product`.`ListPrice` AS `ListPrice`,
        `Product`.`Size` AS `Size`,
        `Product`.`Weight` AS `Weight`,
        `Product`.`ProductCategoryID` AS `ProductCategoryID`,
        `Product`.`ProductModelID` AS `ProductModelID`,
        `Product`.`SellStartDate` AS `SellStartDate`,
        `Product`.`SellEndDate` AS `SellEndDate`,
        `Product`.`DiscontinuedDate` AS `DiscontinuedDate`,
        `Product`.`ThumbNailPhoto` AS `ThumbNailPhoto`,
        `Product`.`ThumbnailPhotoFileName` AS `ThumbnailPhotoFileName`,
        `Product`.`RowGuid` AS `RowGuid`,
        `Product`.`ModifiedDate` AS `ModifiedDate`
    FROM `Product`
    WHERE (`Product`.`ProductID` < 720))
    UNION ALL
    (SELECT
        `Product`.`ProductID` AS `ProductID`,
        `Product`.`Name` AS `Name`,
        `Product`.`ProductNumber` AS `ProductNumber`,
        `Product`.`Color` AS `Color`,
        `Product`.`StandardCost` AS `StandardCost`,
        `Product`.`ListPrice` AS `ListPrice`,
        `Product`.`Size` AS `Size`,
        `Product`.`Weight` AS `Weight`,
        `Product`.`ProductCategoryID` AS `ProductCategoryID`,
        `Product`.`ProductModelID` AS `ProductModelID`,
        `Product`.`SellStartDate` AS `SellStartDate`,
        `Product`.`SellEndDate` AS `SellEndDate`,
        `Product`.`DiscontinuedDate` AS `DiscontinuedDate`,
        `Product`.`ThumbNailPhoto` AS `ThumbNailPhoto`,
        `Product`.`ThumbnailPhotoFileName` AS `ThumbnailPhotoFileName`,
        `Product`.`RowGuid` AS `RowGuid`,
        `Product`.`ModifiedDate` AS `ModifiedDate`
    FROM `Product`
    WHERE (`Product`.`ProductID` > 800))) `Product`
ORDER BY `Product`.`Name` ASC;
";
                Assert.AreEqual(expectedSql, query2.ToString());
            }
        }

        [TestMethod]
        public void DbSet_Union_multiple()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var query = db.Product.Where(x => x.ProductID < _Int32.Const(720))
                    .UnionAll(db.Product.Where(x => x.ProductID > _Int32.Const(800) & x.ProductID < _Int32.Const(900)))
                    .UnionAll(db.Product.Where(x => x.ProductID > _Int32.Const(900)));
                var expectedSql =
@"((SELECT
    `Product`.`ProductID` AS `ProductID`,
    `Product`.`Name` AS `Name`,
    `Product`.`ProductNumber` AS `ProductNumber`,
    `Product`.`Color` AS `Color`,
    `Product`.`StandardCost` AS `StandardCost`,
    `Product`.`ListPrice` AS `ListPrice`,
    `Product`.`Size` AS `Size`,
    `Product`.`Weight` AS `Weight`,
    `Product`.`ProductCategoryID` AS `ProductCategoryID`,
    `Product`.`ProductModelID` AS `ProductModelID`,
    `Product`.`SellStartDate` AS `SellStartDate`,
    `Product`.`SellEndDate` AS `SellEndDate`,
    `Product`.`DiscontinuedDate` AS `DiscontinuedDate`,
    `Product`.`ThumbNailPhoto` AS `ThumbNailPhoto`,
    `Product`.`ThumbnailPhotoFileName` AS `ThumbnailPhotoFileName`,
    `Product`.`RowGuid` AS `RowGuid`,
    `Product`.`ModifiedDate` AS `ModifiedDate`
FROM `Product`
WHERE (`Product`.`ProductID` < 720))
UNION ALL
(SELECT
    `Product`.`ProductID` AS `ProductID`,
    `Product`.`Name` AS `Name`,
    `Product`.`ProductNumber` AS `ProductNumber`,
    `Product`.`Color` AS `Color`,
    `Product`.`StandardCost` AS `StandardCost`,
    `Product`.`ListPrice` AS `ListPrice`,
    `Product`.`Size` AS `Size`,
    `Product`.`Weight` AS `Weight`,
    `Product`.`ProductCategoryID` AS `ProductCategoryID`,
    `Product`.`ProductModelID` AS `ProductModelID`,
    `Product`.`SellStartDate` AS `SellStartDate`,
    `Product`.`SellEndDate` AS `SellEndDate`,
    `Product`.`DiscontinuedDate` AS `DiscontinuedDate`,
    `Product`.`ThumbNailPhoto` AS `ThumbNailPhoto`,
    `Product`.`ThumbnailPhotoFileName` AS `ThumbnailPhotoFileName`,
    `Product`.`RowGuid` AS `RowGuid`,
    `Product`.`ModifiedDate` AS `ModifiedDate`
FROM `Product`
WHERE ((`Product`.`ProductID` > 800) AND (`Product`.`ProductID` < 900))))
UNION ALL
(SELECT
    `Product`.`ProductID` AS `ProductID`,
    `Product`.`Name` AS `Name`,
    `Product`.`ProductNumber` AS `ProductNumber`,
    `Product`.`Color` AS `Color`,
    `Product`.`StandardCost` AS `StandardCost`,
    `Product`.`ListPrice` AS `ListPrice`,
    `Product`.`Size` AS `Size`,
    `Product`.`Weight` AS `Weight`,
    `Product`.`ProductCategoryID` AS `ProductCategoryID`,
    `Product`.`ProductModelID` AS `ProductModelID`,
    `Product`.`SellStartDate` AS `SellStartDate`,
    `Product`.`SellEndDate` AS `SellEndDate`,
    `Product`.`DiscontinuedDate` AS `DiscontinuedDate`,
    `Product`.`ThumbNailPhoto` AS `ThumbNailPhoto`,
    `Product`.`ThumbnailPhotoFileName` AS `ThumbnailPhotoFileName`,
    `Product`.`RowGuid` AS `RowGuid`,
    `Product`.`ModifiedDate` AS `ModifiedDate`
FROM `Product`
WHERE (`Product`.`ProductID` > 900));
";
                Assert.AreEqual(expectedSql, query.ToString());

                var query2 = query.OrderBy(x => x.Name.Asc());
                expectedSql =
@"SELECT
    `Product`.`ProductID` AS `ProductID`,
    `Product`.`Name` AS `Name`,
    `Product`.`ProductNumber` AS `ProductNumber`,
    `Product`.`Color` AS `Color`,
    `Product`.`StandardCost` AS `StandardCost`,
    `Product`.`ListPrice` AS `ListPrice`,
    `Product`.`Size` AS `Size`,
    `Product`.`Weight` AS `Weight`,
    `Product`.`ProductCategoryID` AS `ProductCategoryID`,
    `Product`.`ProductModelID` AS `ProductModelID`,
    `Product`.`SellStartDate` AS `SellStartDate`,
    `Product`.`SellEndDate` AS `SellEndDate`,
    `Product`.`DiscontinuedDate` AS `DiscontinuedDate`,
    `Product`.`ThumbNailPhoto` AS `ThumbNailPhoto`,
    `Product`.`ThumbnailPhotoFileName` AS `ThumbnailPhotoFileName`,
    `Product`.`RowGuid` AS `RowGuid`,
    `Product`.`ModifiedDate` AS `ModifiedDate`
FROM
    (((SELECT
        `Product`.`ProductID` AS `ProductID`,
        `Product`.`Name` AS `Name`,
        `Product`.`ProductNumber` AS `ProductNumber`,
        `Product`.`Color` AS `Color`,
        `Product`.`StandardCost` AS `StandardCost`,
        `Product`.`ListPrice` AS `ListPrice`,
        `Product`.`Size` AS `Size`,
        `Product`.`Weight` AS `Weight`,
        `Product`.`ProductCategoryID` AS `ProductCategoryID`,
        `Product`.`ProductModelID` AS `ProductModelID`,
        `Product`.`SellStartDate` AS `SellStartDate`,
        `Product`.`SellEndDate` AS `SellEndDate`,
        `Product`.`DiscontinuedDate` AS `DiscontinuedDate`,
        `Product`.`ThumbNailPhoto` AS `ThumbNailPhoto`,
        `Product`.`ThumbnailPhotoFileName` AS `ThumbnailPhotoFileName`,
        `Product`.`RowGuid` AS `RowGuid`,
        `Product`.`ModifiedDate` AS `ModifiedDate`
    FROM `Product`
    WHERE (`Product`.`ProductID` < 720))
    UNION ALL
    (SELECT
        `Product`.`ProductID` AS `ProductID`,
        `Product`.`Name` AS `Name`,
        `Product`.`ProductNumber` AS `ProductNumber`,
        `Product`.`Color` AS `Color`,
        `Product`.`StandardCost` AS `StandardCost`,
        `Product`.`ListPrice` AS `ListPrice`,
        `Product`.`Size` AS `Size`,
        `Product`.`Weight` AS `Weight`,
        `Product`.`ProductCategoryID` AS `ProductCategoryID`,
        `Product`.`ProductModelID` AS `ProductModelID`,
        `Product`.`SellStartDate` AS `SellStartDate`,
        `Product`.`SellEndDate` AS `SellEndDate`,
        `Product`.`DiscontinuedDate` AS `DiscontinuedDate`,
        `Product`.`ThumbNailPhoto` AS `ThumbNailPhoto`,
        `Product`.`ThumbnailPhotoFileName` AS `ThumbnailPhotoFileName`,
        `Product`.`RowGuid` AS `RowGuid`,
        `Product`.`ModifiedDate` AS `ModifiedDate`
    FROM `Product`
    WHERE ((`Product`.`ProductID` > 800) AND (`Product`.`ProductID` < 900))))
    UNION ALL
    (SELECT
        `Product`.`ProductID` AS `ProductID`,
        `Product`.`Name` AS `Name`,
        `Product`.`ProductNumber` AS `ProductNumber`,
        `Product`.`Color` AS `Color`,
        `Product`.`StandardCost` AS `StandardCost`,
        `Product`.`ListPrice` AS `ListPrice`,
        `Product`.`Size` AS `Size`,
        `Product`.`Weight` AS `Weight`,
        `Product`.`ProductCategoryID` AS `ProductCategoryID`,
        `Product`.`ProductModelID` AS `ProductModelID`,
        `Product`.`SellStartDate` AS `SellStartDate`,
        `Product`.`SellEndDate` AS `SellEndDate`,
        `Product`.`DiscontinuedDate` AS `DiscontinuedDate`,
        `Product`.`ThumbNailPhoto` AS `ThumbNailPhoto`,
        `Product`.`ThumbnailPhotoFileName` AS `ThumbnailPhotoFileName`,
        `Product`.`RowGuid` AS `RowGuid`,
        `Product`.`ModifiedDate` AS `ModifiedDate`
    FROM `Product`
    WHERE (`Product`.`ProductID` > 900))) `Product`
ORDER BY `Product`.`Name` ASC;
";
                Assert.AreEqual(expectedSql, query2.ToString());
            }
        }

        [TestMethod]
        public void DbSet_offset_fetch()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var query = db.SalesOrderDetail.OrderBy(10, 20, x => x.SalesOrderDetailID);
                var expectedSql =
@"SELECT
    `SalesOrderDetail`.`SalesOrderID` AS `SalesOrderID`,
    `SalesOrderDetail`.`SalesOrderDetailID` AS `SalesOrderDetailID`,
    `SalesOrderDetail`.`OrderQty` AS `OrderQty`,
    `SalesOrderDetail`.`ProductID` AS `ProductID`,
    `SalesOrderDetail`.`UnitPrice` AS `UnitPrice`,
    `SalesOrderDetail`.`UnitPriceDiscount` AS `UnitPriceDiscount`,
    `SalesOrderDetail`.`LineTotal` AS `LineTotal`,
    `SalesOrderDetail`.`RowGuid` AS `RowGuid`,
    `SalesOrderDetail`.`ModifiedDate` AS `ModifiedDate`
FROM `SalesOrderDetail`
ORDER BY `SalesOrderDetail`.`SalesOrderDetailID`
LIMIT 10, 20;
";
                Assert.AreEqual(expectedSql, query.ToString());
            }
        }
    }
}
