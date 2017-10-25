using DevZest.Data;
using DevZest.Data.SqlServer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Samples.AdventureWorksLT
{
    public class Db : SqlSession
    {
        public Db(string connectionString, Action<Db> initializer = null)
            : base(CreateSqlConnection(connectionString))
        {
            if (initializer != null)
                initializer(this);
        }

        private static SqlConnection CreateSqlConnection(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));
            return new SqlConnection(connectionString);
        }
#if DEBUG
        // For unit tests
        public static Db New(SqlVersion sqlVersion)
        {
            var result = new Db(new SqlConnection());
            result.SqlVersion = sqlVersion;
            return result;
        }
#endif

        public Db(SqlConnection sqlConnection)
            : base(sqlConnection)
        {
        }

        protected Db(SqlVersion sqlVersion)
            : base(new SqlConnection())
        {
            SqlVersion = sqlVersion;
        }

        private DbTable<Address> _addresses;
        public DbTable<Address> Addresses
        {
            get { return GetTable(ref _addresses, "[SalesLT].[Address]"); }
        }

        private DbTable<Customer> _customers;
        public DbTable<Customer> Customers
        {
            get { return GetTable(ref _customers, "[SalesLT].[Customer]"); }
        }

        private DbTable<CustomerAddress> _customerAddresses;
        public DbTable<CustomerAddress> CustomerAddresses
        {
            get
            {
                return GetTable(ref _customerAddresses, "[SalesLT].[CustomerAddress]",
                    _ => ForeignKey(null, _.Customer, Customers._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction),
                    _ => ForeignKey(null, _.Address, Addresses._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction));
            }
        }

        private DbTable<ProductCategory> _productCategories;
        public DbTable<ProductCategory> ProductCategories
        {
            get
            {
                return GetTable(ref _productCategories, "[SalesLT].[ProductCategory]",
                    _ => ForeignKey(null, _.ParentProductCategory, _, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction));
            }
        }

        private DbTable<ProductModel> _productModels;
        public DbTable<ProductModel> ProductModels
        {
            get { return GetTable(ref _productModels, "[SalesLT].[ProductModel]"); }
        }

        private DbTable<ProductDescription> _productDescriptions;
        public DbTable<ProductDescription> ProductDescriptions
        {
            get { return GetTable(ref _productDescriptions, "[SalesLT].[ProductDescription]"); }
        }

        private DbTable<ProductModelProductDescription> _productModelProductDescriptions;
        public DbTable<ProductModelProductDescription> ProductModelProductDescriptions
        {
            get
            {
                return GetTable(ref _productModelProductDescriptions, "[SalesLT].[ProductModelProductDescription]",
                    _ => ForeignKey(null, _.ProductModel, ProductModels._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction),
                    _ => ForeignKey(null, _.ProductDescription, ProductDescriptions._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction));
            }
        }

        private DbTable<Product> _products;
        public DbTable<Product> Products
        {
            get
            {
                return GetTable(ref _products, "[SalesLT].[Product]",
                    _ => ForeignKey(null, _.ProductModel, ProductModels._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction),
                    _ => ForeignKey(null, _.ProductCategory, ProductCategories._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction));
            }
        }

        private DbTable<SalesOrder> _salesOrders;
        public DbTable<SalesOrder> SalesOrders
        {
            get
            {
                return GetTable(ref _salesOrders, "[SalesLT].[SalesOrderHeader]",
                    _ => ForeignKey(null, _.Customer, Customers._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction),
                    _ => ForeignKey(null, _.BillToCustomerAddress, CustomerAddresses._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction),
                    _ => ForeignKey(null, _.ShipToCustomerAddress, CustomerAddresses._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction));
            }
        }

        private DbTable<SalesOrderDetail> _salesOrderDetails;
        public DbTable<SalesOrderDetail> SalesOrderDetails
        {
            get
            {
                return GetTable(ref _salesOrderDetails, "[SalesLT].[SalesOrderDetail]",
                    _ => ForeignKey(null, _.SalesOrder, SalesOrders._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction),
                    _ => ForeignKey(null, _.Product, Products._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction));
            }
        }

        public DbSet<SalesOrder> GetSalesOrderList(string filterText, IReadOnlyList<IColumnComparer> orderBy)
        {
            DbSet<SalesOrder> result;
            if (string.IsNullOrEmpty(filterText))
                result = SalesOrders;
            else
                result = SalesOrders.Where(_ => _.SalesOrderNumber.Contains(filterText) | _.PurchaseOrderNumber.Contains(filterText));

            if (orderBy != null && orderBy.Count > 0)
                result = result.OrderBy(GetOrderBy(result._, orderBy));

            return result;
        }

        private static ColumnSort[] GetOrderBy(Model model, IReadOnlyList<IColumnComparer> orderBy)
        {
            Debug.Assert(orderBy != null && orderBy.Count > 0);
            var result = new ColumnSort[orderBy.Count];
            for (int i = 0; i < orderBy.Count; i++)
            {
                var column = orderBy[i].GetColumn(model);
                var direction = orderBy[i].Direction;
                result[i] = direction == SortDirection.Descending ? column.Desc() : column.Asc();
            }
            return result;
        }

        public DbSet<SalesOrderToEdit> GetSalesOrderToEdit(int salesOrderID)
        {
            var result = CreateQuery((DbQueryBuilder builder, SalesOrderToEdit _) =>
            {
                var ext = _.GetExtension<SalesOrderToEdit.Ext>();
                Debug.Assert(ext != null);
                SalesOrder o;
                Customer c;
                Address shipTo, billTo;
                builder.From(SalesOrders, out o)
                    .InnerJoin(Customers, o.Customer, out c)
                    .InnerJoin(Addresses, o.ShipToAddress, out shipTo)
                    .InnerJoin(Addresses, o.BillToAddress, out billTo)
                    .AutoSelect()
                    .AutoSelect(shipTo, ext.ShipToAddress)
                    .AutoSelect(billTo, ext.BillToAddress)
                    .Where(o.SalesOrderID == _Int32.Param(salesOrderID));
            });

            result.CreateChild(_ => _.SalesOrderDetails, (DbQueryBuilder builder, SalesOrderDetail _) =>
            {
                Debug.Assert(_.GetExtension<SalesOrderToEdit.DetailExt>() != null);
                SalesOrderDetail d;
                Product p;
                builder.From(SalesOrderDetails, out d)
                    .InnerJoin(Products, d.Product, out p)
                    .AutoSelect();
            });

            return result;
        }
    }
}
