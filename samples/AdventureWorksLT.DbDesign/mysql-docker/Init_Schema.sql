SET FOREIGN_KEY_CHECKS = 0;

DROP SCHEMA IF EXISTS AdventureWorksLT ;
CREATE SCHEMA IF NOT EXISTS AdventureWorksLT ;

-- ----------------------------------------------------------------------------
-- Table AdventureWorksLT.BuildVersion
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS AdventureWorksLT.`BuildVersion` (
  `SystemInformationID` TINYINT UNSIGNED NOT NULL COMMENT 'Primary key for BuildVersion records.',
  `Database Version` VARCHAR(25) CHARACTER SET 'utf8mb4' NOT NULL COMMENT 'Version number of the database in 9.yy.mm.dd.00 format.',
  `VersionDate` DATETIME(6) NOT NULL COMMENT 'Date and time the record was last updated.',
  `ModifiedDate` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Date and time the record was last updated.')
COMMENT = 'Current version number of the AdventureWorksLT 2012 sample database. ';

-- ----------------------------------------------------------------------------
-- Table AdventureWorksLT.Address
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS AdventureWorksLT.`Address` (
  `AddressID` INT NOT NULL AUTO_INCREMENT COMMENT 'Primary key for Address records.',
  `AddressLine1` VARCHAR(60) CHARACTER SET 'utf8mb4' NOT NULL COMMENT 'First street address line.',
  `AddressLine2` VARCHAR(60) CHARACTER SET 'utf8mb4' NULL COMMENT 'Second street address line.',
  `City` VARCHAR(30) CHARACTER SET 'utf8mb4' NOT NULL COMMENT 'Name of the city.',
  `StateProvince` VARCHAR(100) CHARACTER SET 'utf8mb4' NOT NULL COMMENT 'Name of state or province.',
  `CountryRegion` VARCHAR(100) CHARACTER SET 'utf8mb4' NOT NULL,
  `PostalCode` VARCHAR(15) CHARACTER SET 'utf8mb4' NOT NULL COMMENT 'Postal code for the street address.',
  `rowguid` CHAR(36) NOT NULL COLLATE ascii_general_ci COMMENT 'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
  `ModifiedDate` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Date and time the record was last updated.',
  PRIMARY KEY (`AddressID`),
  UNIQUE INDEX `AK_Address_rowguid` (`rowguid` ASC),
  INDEX `IX_Address_AddressLine1_AddressLine2_City_StateProvince_Postal3` (`AddressLine1` ASC, `AddressLine2` ASC, `City` ASC, `StateProvince` ASC, `PostalCode` ASC, `CountryRegion` ASC),
  INDEX `IX_Address_StateProvince` (`StateProvince` ASC))
COMMENT = 'Street address information for customers.';

-- ----------------------------------------------------------------------------
-- Table AdventureWorksLT.Customer
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS AdventureWorksLT.`Customer` (
  `CustomerID` INT NOT NULL AUTO_INCREMENT COMMENT 'Primary key for Customer records.',
  `NameStyle` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '0 = The data in FirstName and LastName are stored in western style (first name, last name) order.  1 = Eastern style (last name, first name) order.',
  `Title` VARCHAR(8) CHARACTER SET 'utf8mb4' NULL COMMENT 'A courtesy title. For example, Mr. or Ms.',
  `FirstName` VARCHAR(100) CHARACTER SET 'utf8mb4' NOT NULL COMMENT 'First name of the person.',
  `MiddleName` VARCHAR(100) CHARACTER SET 'utf8mb4' NULL COMMENT 'Middle name or middle initial of the person.',
  `LastName` VARCHAR(100) CHARACTER SET 'utf8mb4' NOT NULL COMMENT 'Last name of the person.',
  `Suffix` VARCHAR(10) CHARACTER SET 'utf8mb4' NULL COMMENT 'Surname suffix. For example, Sr. or Jr.',
  `CompanyName` VARCHAR(128) CHARACTER SET 'utf8mb4' NULL COMMENT 'The customer\'s organization.',
  `SalesPerson` VARCHAR(256) CHARACTER SET 'utf8mb4' NULL COMMENT 'The customer\'s sales person, an employee of AdventureWorks Cycles.',
  `EmailAddress` VARCHAR(50) CHARACTER SET 'utf8mb4' NULL COMMENT 'E-mail address for the person.',
  `Phone` VARCHAR(50) CHARACTER SET 'utf8mb4' NULL COMMENT 'Phone number associated with the person.',
  `PasswordHash` VARCHAR(128) NOT NULL COMMENT 'Password for the e-mail account.',
  `PasswordSalt` VARCHAR(10) NOT NULL COMMENT 'Random value concatenated with the password string before the password is hashed.',
  `rowguid` CHAR(36) NOT NULL COLLATE ascii_general_ci COMMENT 'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
  `ModifiedDate` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Date and time the record was last updated.',
  PRIMARY KEY (`CustomerID`),
  UNIQUE INDEX `AK_Customer_rowguid` (`rowguid` ASC),
  INDEX `IX_Customer_EmailAddress` (`EmailAddress` ASC))
COMMENT = 'Customer information.';

-- ----------------------------------------------------------------------------
-- Table AdventureWorksLT.CustomerAddress
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS AdventureWorksLT.`CustomerAddress` (
  `CustomerID` INT NOT NULL COMMENT 'Primary key. Foreign key to Customer.CustomerID.',
  `AddressID` INT NOT NULL COMMENT 'Primary key. Foreign key to Address.AddressID.',
  `AddressType` VARCHAR(100) CHARACTER SET 'utf8mb4' NOT NULL COMMENT 'The kind of Address. One of: Archive, Billing, Home, Main Office, Primary, Shipping',
  `rowguid` CHAR(36) NOT NULL COLLATE ascii_general_ci COMMENT 'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
  `ModifiedDate` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Date and time the record was last updated.',
  PRIMARY KEY (`CustomerID`, `AddressID`),
  UNIQUE INDEX `AK_CustomerAddress_rowguid` (`rowguid` ASC),
  CONSTRAINT `FK_CustomerAddress_Customer_CustomerID`
    FOREIGN KEY (`CustomerID`)
    REFERENCES AdventureWorksLT.`Customer` (`CustomerID`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_CustomerAddress_Address_AddressID`
    FOREIGN KEY (`AddressID`)
    REFERENCES AdventureWorksLT.`Address` (`AddressID`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
COMMENT = 'Cross-reference table mapping customers to their address(es).';

-- ----------------------------------------------------------------------------
-- Table AdventureWorksLT.Product
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS AdventureWorksLT.`Product` (
  `ProductID` INT NOT NULL AUTO_INCREMENT COMMENT 'Primary key for Product records.',
  `Name` VARCHAR(100) CHARACTER SET 'utf8mb4' NOT NULL COMMENT 'Name of the product.',
  `ProductNumber` VARCHAR(25) CHARACTER SET 'utf8mb4' NOT NULL COMMENT 'Unique product identification number.',
  `Color` VARCHAR(15) CHARACTER SET 'utf8mb4' NULL COMMENT 'Product color.',
  `StandardCost` DECIMAL(19,4) NOT NULL COMMENT 'Standard cost of the product.',
  `ListPrice` DECIMAL(19,4) NOT NULL COMMENT 'Selling price.',
  `Size` VARCHAR(5) CHARACTER SET 'utf8mb4' NULL COMMENT 'Product size.',
  `Weight` DECIMAL(8,2) NULL COMMENT 'Product weight.',
  `ProductCategoryID` INT NULL COMMENT 'Product is a member of this product category. Foreign key to ProductCategory.ProductCategoryID. ',
  `ProductModelID` INT NULL COMMENT 'Product is a member of this product model. Foreign key to ProductModel.ProductModelID.',
  `SellStartDate` DATETIME(6) NOT NULL COMMENT 'Date the product was available for sale.',
  `SellEndDate` DATETIME(6) NULL COMMENT 'Date the product was no longer available for sale.',
  `DiscontinuedDate` DATETIME(6) NULL COMMENT 'Date the product was discontinued.',
  `ThumbNailPhoto` LONGBLOB NULL COMMENT 'Small image of the product.',
  `ThumbnailPhotoFileName` VARCHAR(50) CHARACTER SET 'utf8mb4' NULL COMMENT 'Small image file name.',
  `rowguid` CHAR(36) NOT NULL COLLATE ascii_general_ci COMMENT 'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
  `ModifiedDate` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Date and time the record was last updated.',
  PRIMARY KEY (`ProductID`),
  UNIQUE INDEX `AK_Product_rowguid` (`rowguid` ASC),
  UNIQUE INDEX `AK_Product_Name` (`Name` ASC),
  UNIQUE INDEX `AK_Product_ProductNumber` (`ProductNumber` ASC),
  CONSTRAINT `FK_Product_ProductModel_ProductModelID`
    FOREIGN KEY (`ProductModelID`)
    REFERENCES AdventureWorksLT.`ProductModel` (`ProductModelID`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_Product_ProductCategory_ProductCategoryID`
    FOREIGN KEY (`ProductCategoryID`)
    REFERENCES AdventureWorksLT.`ProductCategory` (`ProductCategoryID`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
COMMENT = 'Products sold or used in the manfacturing of sold products.';

-- ----------------------------------------------------------------------------
-- Table AdventureWorksLT.ProductCategory
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS AdventureWorksLT.`ProductCategory` (
  `ProductCategoryID` INT NOT NULL AUTO_INCREMENT COMMENT 'Primary key for ProductCategory records.',
  `ParentProductCategoryID` INT NULL COMMENT 'Product category identification number of immediate ancestor category. Foreign key to ProductCategory.ProductCategoryID.',
  `Name` VARCHAR(100) CHARACTER SET 'utf8mb4' NOT NULL COMMENT 'Category description.',
  `rowguid` CHAR(36) NOT NULL COLLATE ascii_general_ci COMMENT 'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
  `ModifiedDate` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Date and time the record was last updated.',
  PRIMARY KEY (`ProductCategoryID`),
  UNIQUE INDEX `AK_ProductCategory_rowguid` (`rowguid` ASC),
  UNIQUE INDEX `AK_ProductCategory_Name` (`Name` ASC),
  CONSTRAINT `FK_ProductCategory_ProductCategory_ParentProductCategoryID_Pro4`
    FOREIGN KEY (`ParentProductCategoryID`)
    REFERENCES AdventureWorksLT.`ProductCategory` (`ProductCategoryID`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
COMMENT = 'High-level product categorization.';

-- ----------------------------------------------------------------------------
-- Table AdventureWorksLT.ProductDescription
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS AdventureWorksLT.`ProductDescription` (
  `ProductDescriptionID` INT NOT NULL AUTO_INCREMENT COMMENT 'Primary key for ProductDescription records.',
  `Description` VARCHAR(400) CHARACTER SET 'utf8mb4' NOT NULL COMMENT 'Description of the product.',
  `rowguid` CHAR(36) NOT NULL COLLATE ascii_general_ci COMMENT 'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
  `ModifiedDate` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Date and time the record was last updated.',
  PRIMARY KEY (`ProductDescriptionID`),
  UNIQUE INDEX `AK_ProductDescription_rowguid` (`rowguid` ASC))
COMMENT = 'Product descriptions in several languages.';

-- ----------------------------------------------------------------------------
-- Table AdventureWorksLT.ProductModel
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS AdventureWorksLT.`ProductModel` (
  `ProductModelID` INT NOT NULL AUTO_INCREMENT,
  `Name` VARCHAR(100) CHARACTER SET 'utf8mb4' NOT NULL,
  `CatalogDescription` TEXT NULL,
  `rowguid` CHAR(36) NOT NULL COLLATE ascii_general_ci,
  `ModifiedDate` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`ProductModelID`),
  UNIQUE INDEX `AK_ProductModel_rowguid` (`rowguid` ASC),
  UNIQUE INDEX `AK_ProductModel_Name` (`Name` ASC),
  INDEX `PXML_ProductModel_CatalogDescription` (`CatalogDescription`(255) ASC));

-- ----------------------------------------------------------------------------
-- Table AdventureWorksLT.ProductModelProductDescription
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS AdventureWorksLT.`ProductModelProductDescription` (
  `ProductModelID` INT NOT NULL COMMENT 'Primary key. Foreign key to ProductModel.ProductModelID.',
  `ProductDescriptionID` INT NOT NULL COMMENT 'Primary key. Foreign key to ProductDescription.ProductDescriptionID.',
  `Culture` CHAR(6) CHARACTER SET 'utf8mb4' NOT NULL COMMENT 'The culture for which the description is written',
  `rowguid` CHAR(36) NOT NULL COLLATE ascii_general_ci,
  `ModifiedDate` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Date and time the record was last updated.',
  PRIMARY KEY (`ProductModelID`, `ProductDescriptionID`, `Culture`),
  UNIQUE INDEX `AK_ProductModelProductDescription_rowguid` (`rowguid` ASC),
  CONSTRAINT `FK_ProductModelProductDescription_ProductDescription_ProductDe5`
    FOREIGN KEY (`ProductDescriptionID`)
    REFERENCES AdventureWorksLT.`ProductDescription` (`ProductDescriptionID`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_ProductModelProductDescription_ProductModel_ProductModelID`
    FOREIGN KEY (`ProductModelID`)
    REFERENCES AdventureWorksLT.`ProductModel` (`ProductModelID`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
COMMENT = 'Cross-reference table mapping product descriptions and the language the description is written in.';

-- ----------------------------------------------------------------------------
-- Table AdventureWorksLT.SalesOrderDetail
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS AdventureWorksLT.`SalesOrderDetail` (
  `SalesOrderID` INT NOT NULL COMMENT 'Primary key. Foreign key to SalesOrderHeader.SalesOrderID.',
  `SalesOrderDetailID` INT NOT NULL AUTO_INCREMENT COMMENT 'Primary key. One incremental unique number per product sold.',
  `OrderQty` SMALLINT NOT NULL COMMENT 'Quantity ordered per product.',
  `ProductID` INT NOT NULL COMMENT 'Product sold to customer. Foreign key to Product.ProductID.',
  `UnitPrice` DECIMAL(19,4) NOT NULL COMMENT 'Selling price of a single product.',
  `UnitPriceDiscount` DECIMAL(19,4) NOT NULL DEFAULT 0.0 COMMENT 'Discount amount.',
  `LineTotal` DECIMAL(38,6) generated always as (ifnull(UnitPrice*(1-UnitPriceDiscount)*OrderQty,0)) NOT NULL COMMENT 'Per product subtotal. Computed as UnitPrice * (1 - UnitPriceDiscount) * OrderQty.',
  `rowguid` CHAR(36) NOT NULL COLLATE ascii_general_ci COMMENT 'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
  `ModifiedDate` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Date and time the record was last updated.',
  PRIMARY KEY (`SalesOrderDetailID`, `SalesOrderID`),
  UNIQUE INDEX `AK_SalesOrderDetail_rowguid` (`rowguid` ASC),
  INDEX `IX_SalesOrderDetail_ProductID` (`ProductID` ASC),
  CONSTRAINT `FK_SalesOrderDetail_SalesOrderHeader_SalesOrderID`
    FOREIGN KEY (`SalesOrderID`)
    REFERENCES AdventureWorksLT.`SalesOrderHeader` (`SalesOrderID`)
    ON DELETE CASCADE
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_SalesOrderDetail_Product_ProductID`
    FOREIGN KEY (`ProductID`)
    REFERENCES AdventureWorksLT.`Product` (`ProductID`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
COMMENT = 'Individual products associated with a specific sales order. See SalesOrderHeader.';

-- ----------------------------------------------------------------------------
-- Table AdventureWorksLT.SalesOrderHeader
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS AdventureWorksLT.`SalesOrderHeader` (
  `SalesOrderID` INT NOT NULL AUTO_INCREMENT COMMENT 'Primary key.',
  `RevisionNumber` TINYINT UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Incremental number to track changes to the sales order over time.',
  `OrderDate` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Dates the sales order was created.',
  `DueDate` DATETIME(6) NOT NULL COMMENT 'Date the order is due to the customer.',
  `ShipDate` DATETIME(6) NULL COMMENT 'Date the order was shipped to the customer.',
  `Status` TINYINT UNSIGNED NOT NULL DEFAULT 1 COMMENT 'Order current status. 1 = In process; 2 = Approved; 3 = Backordered; 4 = Rejected; 5 = Shipped; 6 = Cancelled',
  `OnlineOrderFlag` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '0 = Order placed by sales person. 1 = Order placed online by customer.',
  `SalesOrderNumber` VARCHAR(25) CHARACTER SET 'utf8mb4' NULL COMMENT 'Unique sales order identification number.',
  `PurchaseOrderNumber` VARCHAR(50) CHARACTER SET 'utf8mb4' NULL COMMENT 'Customer purchase order number reference. ',
  `AccountNumber` VARCHAR(30) CHARACTER SET 'utf8mb4' NULL COMMENT 'Financial accounting number reference.',
  `CustomerID` INT NOT NULL COMMENT 'Customer identification number. Foreign key to Customer.CustomerID.',
  `ShipToAddressID` INT NULL COMMENT 'The ID of the location to send goods.  Foreign key to the Address table.',
  `BillToAddressID` INT NULL COMMENT 'The ID of the location to send invoices.  Foreign key to the Address table.',
  `ShipMethod` VARCHAR(50) CHARACTER SET 'utf8mb4' NOT NULL COMMENT 'Shipping method. Foreign key to ShipMethod.ShipMethodID.',
  `CreditCardApprovalCode` VARCHAR(15) NULL COMMENT 'Approval code provided by the credit card company.',
  `SubTotal` DECIMAL(19,4) NOT NULL DEFAULT 0.00 COMMENT 'Sales subtotal. Computed as SUM(SalesOrderDetail.LineTotal)for the appropriate SalesOrderID.',
  `TaxAmt` DECIMAL(19,4) NOT NULL DEFAULT 0.00 COMMENT 'Tax amount.',
  `Freight` DECIMAL(19,4) NOT NULL DEFAULT 0.00 COMMENT 'Shipping cost.',
  `TotalDue` DECIMAL(19,4) GENERATED ALWAYS AS (ifnull(SubTotal+TaxAmt+Freight, 0)) NOT NULL COMMENT 'Total due from customer. Computed as Subtotal + TaxAmt + Freight.',
  `Comment` LONGTEXT CHARACTER SET 'utf8mb4' NULL COMMENT 'Sales representative comments.',
  `rowguid` CHAR(36) NOT NULL COLLATE ascii_general_ci COMMENT 'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
  `ModifiedDate` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Date and time the record was last updated.',
  PRIMARY KEY (`SalesOrderID`),
  UNIQUE INDEX `AK_SalesOrderHeader_rowguid` (`rowguid` ASC),
  UNIQUE INDEX `AK_SalesOrderHeader_SalesOrderNumber` (`SalesOrderNumber` ASC),
  INDEX `IX_SalesOrderHeader_CustomerID` (`CustomerID` ASC),
  CONSTRAINT `FK_SalesOrderHeader_Customer_CustomerID`
    FOREIGN KEY (`CustomerID`)
    REFERENCES AdventureWorksLT.`Customer` (`CustomerID`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_SalesOrderHeader_Address_ShipTo_AddressID`
    FOREIGN KEY (`ShipToAddressID`)
    REFERENCES AdventureWorksLT.`Address` (`AddressID`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_SalesOrderHeader_Address_BillTo_AddressID`
    FOREIGN KEY (`BillToAddressID`)
    REFERENCES AdventureWorksLT.`Address` (`AddressID`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
COMMENT = 'General sales order information.';

SET FOREIGN_KEY_CHECKS = 1;
