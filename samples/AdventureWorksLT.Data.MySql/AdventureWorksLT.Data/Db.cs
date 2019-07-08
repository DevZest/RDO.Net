using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.MySql;
using MySql.Data.MySqlClient;
using System;

namespace DevZest.Samples.AdventureWorksLT
{
    public partial class Db : MySqlSession
    {
        public Db(string connectionString, Action<Db> initializer = null)
            : base(CreateMySqlConnection(connectionString))
        {
            initializer?.Invoke(this);
        }

        private static MySqlConnection CreateMySqlConnection(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));
            return new MySqlConnection(connectionString);
        }
#if DEBUG
        // For unit tests
        public Db(MySqlVersion mySqlVersion)
            : base(new MySqlConnection())
        {
            MySqlVersion = mySqlVersion;
        }
#endif

        public Db(MySqlConnection mySqlConnection)
            : base(mySqlConnection)
        {
        }

        private DbTable<Address> _address;
        [DbTable(Description = "Street address information for customers.")]
        public DbTable<Address> Address
        {
            get { return GetTable(ref _address); }
        }

        private DbTable<Customer> _customer;
        [DbTable(Description = "Customer information.")]
        public DbTable<Customer> Customer
        {
            get { return GetTable(ref _customer); }
        }

        private DbTable<CustomerAddress> _customerAddress;
        [DbTable(Description = "Cross-reference table mapping customers to their address(es).")]
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
        [DbTable(Description = "High-level product categorization.")]
        [Relationship(nameof(FK_ProductCategory_Parent), Description = "Foreign key constraint referencing ProductCategory.ProductCategoryID.")]
        public DbTable<ProductCategory> ProductCategory
        {
            get { return GetTable(ref _productCategory); }
        }

        [_Relationship]
        private KeyMapping FK_ProductCategory_Parent(ProductCategory _)
        {
            return _.FK_ParentProductCategory.Join(_);
        }

        private DbTable<ProductModel> _productModel;
        public DbTable<ProductModel> ProductModel
        {
            get { return GetTable(ref _productModel); }
        }

        private DbTable<ProductDescription> _productDescription;
        [DbTable(Description = "Product descriptions in several languages.")]
        public DbTable<ProductDescription> ProductDescription
        {
            get { return GetTable(ref _productDescription); }
        }

        private DbTable<ProductModelProductDescription> _productModelProductDescription;
        [DbTable(Description = "Cross-reference table mapping product descriptions and the language the description is written in.")]
        [Relationship(nameof(FK_ProductModelProductDescription_ProductModel_ProductModelID), Description = "Foreign key constraint referencing ProductModel.ProductModelID.")]
        [Relationship(nameof(FK_ProductModelProductDescription_ProductDescription), Description = "Foreign key constraint referencing ProductDescription.ProductDescriptionID.")]
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
        private KeyMapping FK_ProductModelProductDescription_ProductDescription(ProductModelProductDescription _)
        {
            return _.FK_ProductDescription.Join(ProductDescription._);
        }

        private DbTable<Product> _product;
        [DbTable(Description = "Products sold or used in the manfacturing of sold products.")]
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
        [DbTable(Description = "General sales order information.")]
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
        [DbTable(Description = "Individual products associated with a specific sales order. See SalesOrderHeader.")]
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
