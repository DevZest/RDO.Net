using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;
using System;
using System.Data.SqlClient;

namespace DevZest.Samples.AdventureWorksLT
{
    public partial class Db : SqlSession
    {
        public Db(string connectionString, Action<Db> initializer = null)
            : base(CreateSqlConnection(connectionString))
        {
            initializer?.Invoke(this);
        }

        private static SqlConnection CreateSqlConnection(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));
            return new SqlConnection(connectionString);
        }
#if !DEPLOY
        // For unit tests
        public Db(SqlVersion sqlVersion)
            : base(new SqlConnection())
        {
            SqlVersion = sqlVersion;
        }
#endif

        public Db(SqlConnection sqlConnection)
            : base(sqlConnection)
        {
        }

        private DbTable<Address> _address;
        [DbTable("[SalesLT].[Address]", Description = "Street address information for customers.")]
        public DbTable<Address> Address
        {
            get { return GetTable(ref _address); }
        }

        private DbTable<Customer> _customer;
        [DbTable("[SalesLT].[Customer]", Description = "Customer information.")]
        public DbTable<Customer> Customer
        {
            get { return GetTable(ref _customer); }
        }

        private DbTable<CustomerAddress> _customerAddress;
        [DbTable("[SalesLT].[CustomerAddress]", Description = "Cross-reference table mapping customers to their address(es).")]
        [Relationship(nameof(FK_CustomerAddress_Customer_CustomerID), Description = "Foreign key constraint referencing Customer.CustomerID.")]
        [Relationship(nameof(FK_CustomerAddress_Address_AddressID), Description = "Foreign key constraint referencing Address.AddressID.")]
        public DbTable<CustomerAddress> CustomerAddress
        {
            get { return GetTable(ref _customerAddress); }
        }

        [_Relationship]
        private KeyMapping FK_CustomerAddress_Customer_CustomerID(CustomerAddress _)
        {
            return _.FK_Customer.Join(Customer._);
        }

        [_Relationship]
        private KeyMapping FK_CustomerAddress_Address_AddressID(CustomerAddress _)
        {
            return _.FK_Address.Join(Address._);
        }

        private DbTable<ProductCategory> _productCategory;
        [DbTable("[SalesLT].[ProductCategory]", Description = "High-level product categorization.")]
        [Relationship(nameof(FK_ProductCategory_ProductCategory_ParentProductCategoryID_ProductCategoryID), Description = "Foreign key constraint referencing ProductCategory.ProductCategoryID.")]
        public DbTable<ProductCategory> ProductCategory
        {
            get { return GetTable(ref _productCategory); }
        }

        [_Relationship]
        private KeyMapping FK_ProductCategory_ProductCategory_ParentProductCategoryID_ProductCategoryID(ProductCategory _)
        {
            return _.FK_ParentProductCategory.Join(_);
        }

        private DbTable<ProductModel> _productModel;
        [DbTable("[SalesLT].[ProductModel]")]
        public DbTable<ProductModel> ProductModel
        {
            get { return GetTable(ref _productModel); }
        }

        private DbTable<ProductDescription> _productDescription;
        [DbTable("[SalesLT].[ProductDescription]", Description = "Product descriptions in several languages.")]
        public DbTable<ProductDescription> ProductDescription
        {
            get { return GetTable(ref _productDescription); }
        }

        private DbTable<ProductModelProductDescription> _productModelProductDescription;
        [DbTable("[SalesLT].[ProductModelProductDescription]", Description = "Cross-reference table mapping product descriptions and the language the description is written in.")]
        [Relationship(nameof(FK_ProductModelProductDescription_ProductModel_ProductModelID), Description = "Foreign key constraint referencing ProductModel.ProductModelID.")]
        [Relationship(nameof(FK_ProductModelProductDescription_ProductDescription_ProductDescriptionID), Description = "Foreign key constraint referencing ProductDescription.ProductDescriptionID.")]
        public DbTable<ProductModelProductDescription> ProductModelProductDescription
        {
            get { return GetTable(ref _productModelProductDescription); }
        }

        [_Relationship]
        private KeyMapping FK_ProductModelProductDescription_ProductModel_ProductModelID(ProductModelProductDescription _)
        {
            return _.FK_ProductModel.Join(ProductModel._);
        }

        [_Relationship]
        private KeyMapping FK_ProductModelProductDescription_ProductDescription_ProductDescriptionID(ProductModelProductDescription _)
        {
            return _.FK_ProductDescription.Join(ProductDescription._);
        }

        private DbTable<Product> _product;
        [DbTable("[SalesLT].[Product]", Description = "Products sold or used in the manfacturing of sold products.")]
        [Relationship(nameof(FK_Product_ProductModel_ProductModelID))]
        [Relationship(nameof(FK_Product_ProductCategory_ProductCategoryID))]
        public DbTable<Product> Product
        {
            get { return GetTable(ref _product); }
        }

        [_Relationship]
        private KeyMapping FK_Product_ProductModel_ProductModelID(Product _)
        {
            return _.FK_ProductModel.Join(ProductModel._);
        }

        [_Relationship]
        private KeyMapping FK_Product_ProductCategory_ProductCategoryID(Product _)
        {
            return _.FK_ProductCategory.Join(ProductCategory._);
        }

        private DbTable<SalesOrderHeader> _salesOrderHeader;
        [DbTable("[SalesLT].[SalesOrderHeader]", Description = "General sales order information.")]
        [Relationship(nameof(FK_SalesOrderHeader_Customer_CustomerID))]
        [Relationship(nameof(FK_SalesOrderHeader_Address_BillTo_AddressID))]
        [Relationship(nameof(FK_SalesOrderHeader_Address_ShipTo_AddressID))]
        public DbTable<SalesOrderHeader> SalesOrderHeader
        {
            get { return GetTable(ref _salesOrderHeader); }
        }

        [_Relationship]
        private KeyMapping FK_SalesOrderHeader_Customer_CustomerID(SalesOrderHeader _)
        {
            return _.FK_Customer.Join(Customer._);
        }

        [_Relationship]
        private KeyMapping FK_SalesOrderHeader_Address_BillTo_AddressID(SalesOrderHeader _)
        {
            return _.FK_BillToCustomerAddress.Join(CustomerAddress._);
        }

        [_Relationship]
        private KeyMapping FK_SalesOrderHeader_Address_ShipTo_AddressID(SalesOrderHeader _)
        {
            return _.FK_ShipToCustomerAddress.Join(CustomerAddress._);
        }

        private DbTable<SalesOrderDetail> _salesOrderDetail;
        [DbTable("[SalesLT].[SalesOrderDetail]", Description = "Individual products associated with a specific sales order. See SalesOrderHeader.")]
        [Relationship(nameof(FK_SalesOrderDetail_SalesOrderHeader))]
        [Relationship(nameof(FK_SalesOrderDetail_Product))]
        public DbTable<SalesOrderDetail> SalesOrderDetail
        {
            get { return GetTable(ref _salesOrderDetail); }
        }

        [_Relationship]
        private KeyMapping FK_SalesOrderDetail_SalesOrderHeader(SalesOrderDetail _)
        {
            return _.FK_SalesOrderHeader.Join(SalesOrderHeader._);
        }

        [_Relationship]
        private KeyMapping FK_SalesOrderDetail_Product(SalesOrderDetail _)
        {
            return _.FK_Product.Join(Product._);
        }
    }
}
