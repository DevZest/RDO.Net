using DevZest.Data;
using DevZest.Data.SqlServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private DbTable<Address> _addresses;
        [Description("Street address information for customers.")]
        public DbTable<Address> Addresses
        {
            get { return GetTable(ref _addresses, "[SalesLT].[Address]"); }
        }

        private DbTable<Customer> _customers;
        [Description("Customer information.")]
        public DbTable<Customer> Customers
        {
            get { return GetTable(ref _customers, "[SalesLT].[Customer]"); }
        }

        private DbTable<CustomerAddress> _customerAddresses;
        [Description("Cross-reference table mapping customers to their address(es).")]
        public DbTable<CustomerAddress> CustomerAddresses
        {
            get
            {
                return GetTable(ref _customerAddresses, "[SalesLT].[CustomerAddress]",
                    _ => DbForeignKey("FK_CustomerAddress_Customer_CustomerID", "Foreign key constraint referencing Customer.CustomerID.", _.Customer, Customers._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction),
                    _ => DbForeignKey("FK_CustomerAddress_Address_AddressID", "Foreign key constraint referencing Address.AddressID.", _.Address, Addresses._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction));
            }
        }

        private DbTable<ProductCategory> _productCategories;
        [Description("High-level product categorization.")]
        public DbTable<ProductCategory> ProductCategories
        {
            get
            {
                return GetTable(ref _productCategories, "[SalesLT].[ProductCategory]",
                    _ => DbForeignKey("FK_ProductCategory_ProductCategory_ParentProductCategoryID_ProductCategoryID",
                        "Foreign key constraint referencing ProductCategory.ProductCategoryID.",
                        _.ParentProductCategory, _, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction));
            }
        }

        private DbTable<ProductModel> _productModels;
        public DbTable<ProductModel> ProductModels
        {
            get { return GetTable(ref _productModels, "[SalesLT].[ProductModel]"); }
        }

        private DbTable<ProductDescription> _productDescriptions;
        [Description("Product descriptions in several languages.")]
        public DbTable<ProductDescription> ProductDescriptions
        {
            get { return GetTable(ref _productDescriptions, "[SalesLT].[ProductDescription]"); }
        }

        private DbTable<ProductModelProductDescription> _productModelProductDescriptions;
        [Description("Cross-reference table mapping product descriptions and the language the description is written in.")]
        public DbTable<ProductModelProductDescription> ProductModelProductDescriptions
        {
            get
            {
                return GetTable(ref _productModelProductDescriptions, "[SalesLT].[ProductModelProductDescription]",
                    _ => DbForeignKey("FK_ProductModelProductDescription_ProductModel_ProductModelID",
                        "Foreign key constraint referencing ProductModel.ProductModelID.",
                        _.ProductModel, ProductModels._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction),
                    _ => DbForeignKey("FK_ProductModelProductDescription_ProductDescription_ProductDescriptionID",
                        "Foreign key constraint referencing ProductDescription.ProductDescriptionID.",
                        _.ProductDescription, ProductDescriptions._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction));
            }
        }

        private DbTable<Product> _products;
        [Description("Products sold or used in the manfacturing of sold products.")]
        public DbTable<Product> Products
        {
            get
            {
                return GetTable(ref _products, "[SalesLT].[Product]",
                    _ => DbForeignKey("FK_Product_ProductModel_ProductModelID", null, _.ProductModel, ProductModels._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction),
                    _ => DbForeignKey("FK_Product_ProductCategory_ProductCategoryID", null, _.ProductCategory, ProductCategories._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction));
            }
        }

        private DbTable<SalesOrder> _salesOrders;
        [Description("General sales order information.")]
        public DbTable<SalesOrder> SalesOrders
        {
            get
            {
                return GetTable(ref _salesOrders, "[SalesLT].[SalesOrderHeader]",
                    _ => DbForeignKey("FK_SalesOrderHeader_Customer_CustomerID", null,
                        _.Customer, Customers._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction),
                    _ => DbForeignKey("FK_SalesOrderHeader_Address_BillTo_AddressID", null,
                        _.BillToCustomerAddress, CustomerAddresses._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction),
                    _ => DbForeignKey("FK_SalesOrderHeader_Address_ShipTo_AddressID", null,
                        _.ShipToCustomerAddress, CustomerAddresses._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction));
            }
        }

        private DbTable<SalesOrderDetail> _salesOrderDetails;
        [Description("Individual products associated with a specific sales order. See SalesOrderHeader.")]
        public DbTable<SalesOrderDetail> SalesOrderDetails
        {
            get
            {
                return GetTable(ref _salesOrderDetails, "[SalesLT].[SalesOrderDetail]",
                    _ => DbForeignKey(null, null, _.SalesOrder, SalesOrders._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction),
                    _ => DbForeignKey(null, null, _.Product, Products._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction));
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
                var ext = _.GetExtender<SalesOrderToEdit.Ext>();
                Debug.Assert(ext != null);
                builder.From(SalesOrders, out var o)
                    .InnerJoin(Customers, o.Customer, out var c)
                    .InnerJoin(Addresses, o.ShipToAddress, out var shipTo)
                    .InnerJoin(Addresses, o.BillToAddress, out var billTo)
                    .AutoSelect()
                    .AutoSelect(shipTo, ext.ShipToAddress)
                    .AutoSelect(billTo, ext.BillToAddress)
                    .Where(o.SalesOrderID == _Int32.Param(salesOrderID));
            });

            result.CreateChild(_ => _.SalesOrderDetails, (DbQueryBuilder builder, SalesOrderDetail _) =>
            {
                Debug.Assert(_.GetExtender<SalesOrderToEdit.DetailExt>() != null);
                builder.From(SalesOrderDetails, out var d)
                    .InnerJoin(Products, d.Product, out var p)
                    .AutoSelect();
            });

            return result;
        }

        public Task UpdateAsync(DataSet<SalesOrder> salesOrders, CancellationToken ct)
        {
            return ExecuteTransactionAsync(() => PerformUpdateAsync(salesOrders, ct));
        }

        private async Task PerformUpdateAsync(DataSet<SalesOrder> salesOrders, CancellationToken ct)
        {
            for (int i = 0; i < salesOrders.Count; i++)
            {
                salesOrders._.RowGuid[i] = Guid.NewGuid();
                salesOrders._.ModifiedDate[i] = DateTime.Now;
            }
            await SalesOrders.UpdateAsync(salesOrders, ct);
            await SalesOrderDetails.DeleteAsync(salesOrders, _ => _.SalesOrder, ct);
            var salesOrderDetails = salesOrders.Children(_ => _.SalesOrderDetails);
            for (int i = 0; i < salesOrderDetails.Count; i++)
            {
                salesOrderDetails._.RowGuid[i] = Guid.NewGuid();
                salesOrderDetails._.ModifiedDate[i] = DateTime.Now;
            }
            await SalesOrderDetails.InsertAsync(salesOrderDetails, null, false, false, ct);
        }

        public Task InsertAsync(DataSet<SalesOrder> salesOrders, CancellationToken ct)
        {
            return ExecuteTransactionAsync(() => PerformInsertAsync(salesOrders, ct));
        }

        private async Task PerformInsertAsync(DataSet<SalesOrder> salesOrders, CancellationToken ct)
        {
            for (int i = 0; i < salesOrders.Count; i++)
            {
                salesOrders._.RowGuid[i] = Guid.NewGuid();
                salesOrders._.ModifiedDate[i] = DateTime.Now;
            }
            await SalesOrders.InsertAsync(salesOrders, null, false, true, ct);
            var salesOrderDetails = salesOrders.Children(_ => _.SalesOrderDetails);
            for (int i = 0; i < salesOrderDetails.Count; i++)
            {
                salesOrderDetails._.RowGuid[i] = Guid.NewGuid();
                salesOrderDetails._.ModifiedDate[i] = DateTime.Now;
            }
            await SalesOrderDetails.InsertAsync(salesOrderDetails, null, false, false, ct);
        }
    }
}
