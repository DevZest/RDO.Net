CREATE TABLE `Address` (
    `AddressID` INT NOT NULL AUTO_INCREMENT COMMENT 'Primary key for Address records.',
    `AddressLine1` VARCHAR(60) NOT NULL COMMENT 'First street address line.',
    `AddressLine2` VARCHAR(60) NULL COMMENT 'Second street address line.',
    `City` VARCHAR(30) NOT NULL COMMENT 'Name of the city.',
    `StateProvince` VARCHAR(50) NULL COMMENT 'Name of state or province.',
    `CountryRegion` VARCHAR(50) NULL,
    `PostalCode` VARCHAR(15) NOT NULL COMMENT 'Postal code for the street address.',
    `RowGuid` CHAR(36) NOT NULL DEFAULT(UUID()) COMMENT 'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
    `ModifiedDate` TIMESTAMP NOT NULL DEFAULT(NOW()) COMMENT 'Date and time the record was last updated.',

    CONSTRAINT `PK_Address_AddressID` PRIMARY KEY (`AddressID`),
    CONSTRAINT `AK_Address_rowguid` UNIQUE (`RowGuid`),
    INDEX `IX_Address_StateProvince` (`StateProvince`),
    INDEX `IX_Address_All_Attributes` (`AddressLine1`, `AddressLine2`, `City`, `StateProvince`, `CountryRegion`)
) COMMENT 'Street address information for customers.';

CREATE TABLE `Customer` (
    `CustomerID` INT NOT NULL AUTO_INCREMENT COMMENT 'Primary key for Customer records.',
    `NameStyle` BIT NOT NULL DEFAULT(0) COMMENT '0 = The data in FirstName and LastName are stored in western style (first name, last name) order.  1 = Eastern style (last name, first name) order.',
    `Title` VARCHAR(8) NULL COMMENT 'A courtesy title. For example, Mr. or Ms.',
    `FirstName` VARCHAR(50) NULL COMMENT 'First name of the person.',
    `MiddleName` VARCHAR(50) NULL COMMENT 'Middle name or middle initial of the person.',
    `LastName` VARCHAR(50) NULL COMMENT 'Last name of the person.',
    `Suffix` VARCHAR(10) NULL COMMENT 'Surname suffix. For example, Sr. or Jr.',
    `CompanyName` VARCHAR(128) NULL COMMENT 'The customer''s organization.',
    `SalesPerson` VARCHAR(255) NULL COMMENT 'The customer''s sales person, an employee of AdventureWorks Cycles.',
    `EmailAddress` VARCHAR(255) NULL COMMENT 'E-mail address for the person.',
    `Phone` VARCHAR(25) NULL COMMENT 'Phone number associated with the person.',
    `PasswordHash` VARCHAR(128) NOT NULL COMMENT 'Password for the e-mail account.',
    `PasswordSalt` VARCHAR(10) NOT NULL COMMENT 'Random value concatenated with the password string before the password is hashed.',
    `RowGuid` CHAR(36) NOT NULL DEFAULT(UUID()) COMMENT 'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
    `ModifiedDate` TIMESTAMP NOT NULL DEFAULT(NOW()) COMMENT 'Date and time the record was last updated.',

    CONSTRAINT `PK_Customer_CustomerID` PRIMARY KEY (`CustomerID`),
    CONSTRAINT `AK_Customer_rowguid` UNIQUE (`RowGuid`),
    INDEX `IX_Customer_EmailAddress` (`EmailAddress`)
) COMMENT 'Customer information.';

CREATE TABLE `CustomerAddress` (
    `CustomerID` INT NOT NULL COMMENT 'Primary key. Foreign key to Customer.CustomerID.',
    `AddressID` INT NOT NULL COMMENT 'Primary key. Foreign key to Address.AddressID.',
    `AddressType` VARCHAR(50) NULL COMMENT 'The kind of Address. One of: Archive, Billing, Home, Main Office, Primary, Shipping',
    `RowGuid` CHAR(36) NOT NULL DEFAULT(UUID()) COMMENT 'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
    `ModifiedDate` TIMESTAMP NOT NULL DEFAULT(NOW()) COMMENT 'Date and time the record was last updated.',

    CONSTRAINT `PK_CustomerAddress_CustomerID_AddressID` PRIMARY KEY (`CustomerID`, `AddressID`),
    CONSTRAINT `AK_CustomerAddress_rowguid` UNIQUE (`RowGuid`),
    CONSTRAINT `FK_CustomerAddress_Customer_CustomerID` FOREIGN KEY (`CustomerID`)
        REFERENCES `Customer` (`CustomerID`)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,
    CONSTRAINT `FK_CustomerAddress_Address_AddressID` FOREIGN KEY (`AddressID`)
        REFERENCES `Address` (`AddressID`)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION
) COMMENT 'Cross-reference table mapping customers to their address(es).';

CREATE TABLE `ProductCategory` (
    `ProductCategoryID` INT NOT NULL AUTO_INCREMENT COMMENT 'Primary key for ProductCategory records.',
    `ParentProductCategoryID` INT NULL COMMENT 'Product category identification number of immediate ancestor category. Foreign key to ProductCategory.ProductCategoryID.',
    `Name` VARCHAR(50) NOT NULL COMMENT 'Category description.',
    `RowGuid` CHAR(36) NOT NULL DEFAULT(UUID()) COMMENT 'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
    `ModifiedDate` TIMESTAMP NOT NULL DEFAULT(NOW()) COMMENT 'Date and time the record was last updated.',

    CONSTRAINT `PK_ProductCategory_ProductCategoryID` PRIMARY KEY (`ProductCategoryID`),
    CONSTRAINT `AK_ProductCategory_rowguid` UNIQUE (`RowGuid`),
    CONSTRAINT `AK_ProductCategory_Name` UNIQUE (`Name`),
    CONSTRAINT `FK_ProductCategory_Parent` FOREIGN KEY (`ParentProductCategoryID`)
        REFERENCES `ProductCategory` (`ProductCategoryID`)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION
) COMMENT 'High-level product categorization.';

CREATE TABLE `ProductModel` (
    `ProductModelID` INT NOT NULL AUTO_INCREMENT,
    `Name` VARCHAR(50) NULL,
    `CatalogDescription` LONGTEXT NULL,
    `RowGuid` CHAR(36) NOT NULL DEFAULT(UUID()) COMMENT 'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
    `ModifiedDate` TIMESTAMP NOT NULL DEFAULT(NOW()) COMMENT 'Date and time the record was last updated.',

    CONSTRAINT `PK_ProductModel_ProductModelID` PRIMARY KEY (`ProductModelID`),
    CONSTRAINT `AK_ProductModel_rowguid` UNIQUE (`RowGuid`)
);

CREATE TABLE `ProductDescription` (
    `ProductDescriptionID` INT NOT NULL AUTO_INCREMENT COMMENT 'Primary key for ProductDescription records.',
    `Description` VARCHAR(400) NOT NULL COMMENT 'Description of the product.',
    `RowGuid` CHAR(36) NOT NULL DEFAULT(UUID()) COMMENT 'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
    `ModifiedDate` TIMESTAMP NOT NULL DEFAULT(NOW()) COMMENT 'Date and time the record was last updated.',

    CONSTRAINT `PK_ProductDescription_ProductDescriptionID` PRIMARY KEY (`ProductDescriptionID`),
    CONSTRAINT `AK_ProductDescription_rowguid` UNIQUE (`RowGuid`)
) COMMENT 'Product descriptions in several languages.';

CREATE TABLE `ProductModelProductDescription` (
    `ProductModelID` INT NOT NULL COMMENT 'Primary key. Foreign key to ProductModel.ProductModelID.',
    `ProductDescriptionID` INT NOT NULL COMMENT 'Primary key. Foreign key to ProductDescription.ProductDescriptionID.',
    `Culture` CHAR(6) NOT NULL COMMENT 'The culture for which the description is written.',
    `RowGuid` CHAR(36) NOT NULL DEFAULT(UUID()) COMMENT 'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
    `ModifiedDate` TIMESTAMP NOT NULL DEFAULT(NOW()) COMMENT 'Date and time the record was last updated.',

    CONSTRAINT `PK_ProductModelProductDescription` PRIMARY KEY (`ProductModelID`, `ProductDescriptionID`, `Culture`),
    CONSTRAINT `AK_ProductModelProductDescription_rowguid` UNIQUE (`RowGuid`),
    CONSTRAINT `FK_ProductModelProductDescription_ProductModel_ProductModelID` FOREIGN KEY (`ProductModelID`)
        REFERENCES `ProductModel` (`ProductModelID`)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,
    CONSTRAINT `FK_ProductModelProductDescription_ProductDescription` FOREIGN KEY (`ProductDescriptionID`)
        REFERENCES `ProductDescription` (`ProductDescriptionID`)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION
) COMMENT 'Cross-reference table mapping product descriptions and the language the description is written in.';

CREATE TABLE `Product` (
    `ProductID` INT NOT NULL AUTO_INCREMENT COMMENT 'Primary key for Product records.',
    `Name` VARCHAR(50) NULL COMMENT 'Name of the product.',
    `ProductNumber` VARCHAR(25) NOT NULL COMMENT 'Unique product identification number.',
    `Color` VARCHAR(15) NULL COMMENT 'Product color.',
    `StandardCost` DECIMAL(19, 4) NOT NULL COMMENT 'Standard cost of the product.',
    `ListPrice` DECIMAL(19, 4) NOT NULL COMMENT 'Selling price.',
    `Size` VARCHAR(5) NULL COMMENT 'Product size.',
    `Weight` DECIMAL(8, 2) NULL COMMENT 'Product weight.',
    `ProductCategoryID` INT NULL COMMENT 'Product is a member of this product category. Foreign key to ProductCategory.ProductCategoryID.',
    `ProductModelID` INT NULL COMMENT 'Product is a member of this product model. Foreign key to ProductModel.ProductModelID.',
    `SellStartDate` DATE NOT NULL COMMENT 'Date the product was available for sale.',
    `SellEndDate` DATE NULL COMMENT 'Date the product was no longer available for sale.',
    `DiscontinuedDate` DATE NULL COMMENT 'Date the product was discontinued.',
    `ThumbNailPhoto` LONGBLOB NULL COMMENT 'Small image of the product.',
    `ThumbnailPhotoFileName` VARCHAR(50) NULL COMMENT 'Small image file name.',
    `RowGuid` CHAR(36) NOT NULL DEFAULT(UUID()) COMMENT 'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
    `ModifiedDate` TIMESTAMP NOT NULL DEFAULT(NOW()) COMMENT 'Date and time the record was last updated.',

    CONSTRAINT `PK_Product_ProductID` PRIMARY KEY (`ProductID`),
    CONSTRAINT `AK_Product_rowguid` UNIQUE (`RowGuid`),
    CONSTRAINT `CK_Product_ListPrice` CHECK (`ListPrice` >= 0),
    CONSTRAINT `CK_Product_SellEndDate` CHECK ((`SellEndDate` >= `SellStartDate`) OR (`SellEndDate` IS NULL)),
    CONSTRAINT `CK_Product_Weight` CHECK (`Weight` >= 0),
    CONSTRAINT `AK_Product_Name` UNIQUE (`Name`),
    CONSTRAINT `AK_Product_ProductNumber` UNIQUE (`ProductNumber`),
    CONSTRAINT `FK_Product_ProductModel_ProductModelID` FOREIGN KEY (`ProductModelID`)
        REFERENCES `ProductModel` (`ProductModelID`)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,
    CONSTRAINT `FK_Product_ProductCategory_ProductCategoryID` FOREIGN KEY (`ProductCategoryID`)
        REFERENCES `ProductCategory` (`ProductCategoryID`)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION
) COMMENT 'Products sold or used in the manfacturing of sold products.';

CREATE TABLE `SalesOrderHeader` (
    `SalesOrderID` INT NOT NULL AUTO_INCREMENT COMMENT 'Primary key.',
    `RevisionNumber` TINYINT NOT NULL DEFAULT(0) COMMENT 'Incremental number to track changes to the sales order over time.',
    `OrderDate` DATE NOT NULL DEFAULT(NOW()) COMMENT 'Dates the sales order was created.',
    `DueDate` DATE NOT NULL COMMENT 'Date the order is due to the customer.',
    `ShipDate` DATE NULL COMMENT 'Date the order was shipped to the customer.',
    `Status` TINYINT NOT NULL DEFAULT(1) COMMENT 'Order current status. 1 = In process; 2 = Approved; 3 = Backordered; 4 = Rejected; 5 = Shipped; 6 = Cancelled',
    `OnlineOrderFlag` BIT NOT NULL DEFAULT(1) COMMENT '0 = Order placed by sales person. 1 = Order placed online by customer.',
    `SalesOrderNumber` VARCHAR(25) NULL COMMENT 'Unique sales order identification number.',
    `PurchaseOrderNumber` VARCHAR(25) NULL COMMENT 'Customer purchase order number reference.',
    `AccountNumber` VARCHAR(15) NULL COMMENT 'Financial accounting number reference.',
    `CustomerID` INT NOT NULL COMMENT 'Customer identification number. Foreign key to Customer.CustomerID.',
    `ShipToAddressID` INT NULL COMMENT 'The ID of the location to send goods.  Foreign key to the Address table.',
    `BillToAddressID` INT NULL COMMENT 'The ID of the location to send invoices.  Foreign key to the Address table.',
    `ShipMethod` VARCHAR(50) NOT NULL COMMENT 'Shipping method. Foreign key to ShipMethod.ShipMethodID.',
    `CreditCardApprovalCode` VARCHAR(15) NULL COMMENT 'Approval code provided by the credit card company.',
    `SubTotal` DECIMAL(19, 4) NOT NULL DEFAULT(0) COMMENT 'Sales subtotal. Computed as SUM(SalesOrderDetail.LineTotal)for the appropriate SalesOrderID.',
    `TaxAmt` DECIMAL(19, 4) NOT NULL DEFAULT(0) COMMENT 'Tax amount.',
    `Freight` DECIMAL(19, 4) NOT NULL DEFAULT(0) COMMENT 'Shipping cost.',
    `TotalDue` DECIMAL(19, 4) AS (IFNULL(((`SubTotal` + `TaxAmt`) + `Freight`), 0)) COMMENT 'Total due from customer. Computed as Subtotal + TaxAmt + Freight.',
    `Comment` VARCHAR(1000) NULL COMMENT 'Sales representative comments.',
    `RowGuid` CHAR(36) NOT NULL DEFAULT(UUID()) COMMENT 'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
    `ModifiedDate` TIMESTAMP NOT NULL DEFAULT(NOW()) COMMENT 'Date and time the record was last updated.',

    CONSTRAINT `PK_SalesOrderHeader_SalesOrderID` PRIMARY KEY (`SalesOrderID`),
    CONSTRAINT `AK_SalesOrderHeader_rowguid` UNIQUE (`RowGuid`),
    CONSTRAINT `CK_SalesOrderHeader_DueDate` CHECK (`DueDate` >= `OrderDate`),
    CONSTRAINT `CK_SalesOrderHeader_Freight` CHECK (`Freight` >= 0),
    CONSTRAINT `CK_SalesOrderHeader_ShipDate` CHECK ((`ShipDate` >= `OrderDate`) OR (`ShipDate` IS NULL)),
    CONSTRAINT `CK_SalesOrderHeader_SubTotal` CHECK (`SubTotal` >= 0),
    CONSTRAINT `CK_SalesOrderHeader_TaxAmt` CHECK (`TaxAmt` >= 0),
    CONSTRAINT `CK_SalesOrderHeader_Status` CHECK ((`Status` >= 1) AND (`Status` <= 6)),
    CONSTRAINT `AK_SalesOrderHeader_SalesOrderNumber` UNIQUE (`SalesOrderNumber`),
    CONSTRAINT `FK_SalesOrderHeader_Customer_CustomerID` FOREIGN KEY (`CustomerID`)
        REFERENCES `Customer` (`CustomerID`)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,
    CONSTRAINT `FK_SalesOrderHeader_Address_BillTo_AddressID` FOREIGN KEY (`CustomerID`, `BillToAddressID`)
        REFERENCES `CustomerAddress` (`CustomerID`, `AddressID`)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,
    CONSTRAINT `FK_SalesOrderHeader_Address_ShipTo_AddressID` FOREIGN KEY (`CustomerID`, `ShipToAddressID`)
        REFERENCES `CustomerAddress` (`CustomerID`, `AddressID`)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,
    INDEX `IX_SalesOrderHeader_CustomerID` (`CustomerID`)
) COMMENT 'General sales order information.';

CREATE TABLE `SalesOrderDetail` (
    `SalesOrderID` INT NOT NULL COMMENT 'Primary key. Foreign key to SalesOrderHeader.SalesOrderID.',
    `SalesOrderDetailID` INT NOT NULL AUTO_INCREMENT COMMENT 'Primary key. One incremental unique number per product sold.',
    `OrderQty` SMALLINT NOT NULL COMMENT 'Quantity ordered per product.',
    `ProductID` INT NOT NULL COMMENT 'Product sold to customer. Foreign key to Product.ProductID.',
    `UnitPrice` DECIMAL(19, 4) NOT NULL COMMENT 'Selling price of a single product.',
    `UnitPriceDiscount` DECIMAL(19, 4) NOT NULL DEFAULT(0) COMMENT 'Discount amount.',
    `LineTotal` DECIMAL(19, 4) AS (IFNULL(((`UnitPrice` * (1 - `UnitPriceDiscount`)) * CAST(`OrderQty` AS DECIMAL(10, 0))), 0)) COMMENT 'Per product subtotal. Computed as UnitPrice * (1 - UnitPriceDiscount) * OrderQty.',
    `RowGuid` CHAR(36) NOT NULL DEFAULT(UUID()) COMMENT 'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
    `ModifiedDate` TIMESTAMP NOT NULL DEFAULT(NOW()) COMMENT 'Date and time the record was last updated.',

    CONSTRAINT `PK_SalesOrderDetail_SalesOrderID_SalesOrderDetailID` PRIMARY KEY (`SalesOrderID`, `SalesOrderDetailID`),
    CONSTRAINT `AK_SalesOrderDetail_rowguid` UNIQUE (`RowGuid`),
    CONSTRAINT `CK_SalesOrderDetail_OrderQty` CHECK (CAST(`OrderQty` AS DECIMAL(10, 0)) > 0),
    CONSTRAINT `CK_SalesOrderDetail_UnitPrice` CHECK (`UnitPrice` >= 0),
    CONSTRAINT `CK_SalesOrderDetail_UnitPriceDiscount` CHECK (`UnitPriceDiscount` >= 0),
    CONSTRAINT `AK_SalesOrderDetail_SalesOrderDetailID` UNIQUE (`SalesOrderDetailID`),
    CONSTRAINT `FK_SalesOrderDetail_SalesOrderHeader` FOREIGN KEY (`SalesOrderID`)
        REFERENCES `SalesOrderHeader` (`SalesOrderID`)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,
    CONSTRAINT `FK_SalesOrderDetail_Product` FOREIGN KEY (`ProductID`)
        REFERENCES `Product` (`ProductID`)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,
    INDEX `IX_SalesOrderDetail_ProductID` (`ProductID`)
) COMMENT 'Individual products associated with a specific sales order. See SalesOrderHeader.';

