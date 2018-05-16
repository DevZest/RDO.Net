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
                    _ => DbForeignKey("FK_CustomerAddress_Customer_CustomerID", "Foreign key constraint referencing Customer.CustomerID.", _.FK_Customer, Customers._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction),
                    _ => DbForeignKey("FK_CustomerAddress_Address_AddressID", "Foreign key constraint referencing Address.AddressID.", _.FK_Address, Addresses._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction));
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
                        _.FK_ParentProductCategory, _, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction));
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
                        _.FK_ProductModel, ProductModels._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction),
                    _ => DbForeignKey("FK_ProductModelProductDescription_ProductDescription_ProductDescriptionID",
                        "Foreign key constraint referencing ProductDescription.ProductDescriptionID.",
                        _.FK_ProductDescription, ProductDescriptions._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction));
            }
        }

        private DbTable<Product> _products;
        [Description("Products sold or used in the manfacturing of sold products.")]
        public DbTable<Product> Products
        {
            get
            {
                return GetTable(ref _products, "[SalesLT].[Product]",
                    _ => DbForeignKey("FK_Product_ProductModel_ProductModelID", null, _.FK_ProductModel, ProductModels._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction),
                    _ => DbForeignKey("FK_Product_ProductCategory_ProductCategoryID", null, _.FK_ProductCategory, ProductCategories._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction));
            }
        }

        private DbTable<SalesOrderHeader> _salesOrderHeaders;
        [Description("General sales order information.")]
        public DbTable<SalesOrderHeader> SalesOrderHeaders
        {
            get
            {
                return GetTable(ref _salesOrderHeaders, "[SalesLT].[SalesOrderHeader]",
                    _ => DbForeignKey("FK_SalesOrderHeader_Customer_CustomerID", null,
                        _.FK_Customer, Customers._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction),
                    _ => DbForeignKey("FK_SalesOrderHeader_Address_BillTo_AddressID", null,
                        _.FK_BillToCustomerAddress, CustomerAddresses._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction),
                    _ => DbForeignKey("FK_SalesOrderHeader_Address_ShipTo_AddressID", null,
                        _.FK_ShipToCustomerAddress, CustomerAddresses._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction));
            }
        }

        private DbTable<SalesOrderDetail> _salesOrderDetails;
        [Description("Individual products associated with a specific sales order. See SalesOrderHeader.")]
        public DbTable<SalesOrderDetail> SalesOrderDetails
        {
            get
            {
                return GetTable(ref _salesOrderDetails, "[SalesLT].[SalesOrderDetail]",
                    _ => DbForeignKey(null, null, _.FK_SalesOrderHeader, SalesOrderHeaders._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction),
                    _ => DbForeignKey(null, null, _.FK_Product, Products._, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction));
            }
        }

        public DbSet<SalesOrderHeader> GetSalesOrderHeaders(string filterText, IReadOnlyList<IColumnComparer> orderBy)
        {
            DbSet<SalesOrderHeader> result;
            if (string.IsNullOrEmpty(filterText))
                result = SalesOrderHeaders;
            else
                result = SalesOrderHeaders.Where(_ => _.SalesOrderNumber.Contains(filterText) | _.PurchaseOrderNumber.Contains(filterText));

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

        public async Task<DbSet<SalesOrderInfo>> GetSalesOrderInfoAsync(int salesOrderID, CancellationToken ct = default(CancellationToken))
        {
            var result = CreateQuery((DbQueryBuilder builder, SalesOrderInfo _) =>
            {
                var ext = _.GetExtraColumns<SalesOrderHeader.FK.Ext>();
                Debug.Assert(ext != null);
                builder.From(SalesOrderHeaders, out var o)
                    .LeftJoin(Customers, o.FK_Customer, out var c)
                    .LeftJoin(Addresses, o.FK_ShipToAddress, out var shipTo)
                    .LeftJoin(Addresses, o.FK_BillToAddress, out var billTo)
                    .AutoSelect()
                    .AutoSelect(shipTo, ext.ShipToAddress)
                    .AutoSelect(billTo, ext.BillToAddress)
                    .Where(o.SalesOrderID == _Int32.Param(salesOrderID));
            });

            await result.CreateChildAsync(_ => _.SalesOrderDetails, (DbQueryBuilder builder, SalesOrderInfoDetail _) =>
            {
                Debug.Assert(_.GetExtraColumns<Product.Lookup>() != null);
                builder.From(SalesOrderDetails, out var d)
                    .LeftJoin(Products, d.FK_Product, out var p)
                    .AutoSelect();
            }, ct);

            return result;
        }

        public Task UpdateAsync(DataSet<SalesOrder> salesOrders, CancellationToken ct)
        {
            return ExecuteTransactionAsync(() => PerformUpdateAsync(salesOrders, ct));
        }

        private async Task PerformUpdateAsync(DataSet<SalesOrder> salesOrders, CancellationToken ct)
        {
            salesOrders._.ResetRowIdentifiers();
            await SalesOrderHeaders.Update(salesOrders).ExecuteAsync(ct);
            await SalesOrderDetails.Delete(salesOrders, (s, _) => s.Match(_.FK_SalesOrderHeader)).ExecuteAsync(ct);
            var salesOrderDetails = salesOrders.Children(_ => _.SalesOrderDetails);
            salesOrderDetails._.ResetRowIdentifiers();
            await SalesOrderDetails.Insert(salesOrderDetails).ExecuteAsync(ct);
        }

        public Task InsertAsync(DataSet<SalesOrder> salesOrders, CancellationToken ct)
        {
            return ExecuteTransactionAsync(() => PerformInsertAsync(salesOrders, ct));
        }

        private async Task PerformInsertAsync(DataSet<SalesOrder> salesOrders, CancellationToken ct)
        {
            salesOrders._.ResetRowIdentifiers();
            await SalesOrderHeaders.Insert(salesOrders, updateIdentity: true).ExecuteAsync(ct);
            var salesOrderDetails = salesOrders.Children(_ => _.SalesOrderDetails);
            salesOrderDetails._.ResetRowIdentifiers();
            await SalesOrderDetails.Insert(salesOrderDetails).ExecuteAsync(ct);
        }

        public async Task<DataSet<Product.Lookup>> LookupAsync(DataSet<Product.Ref> refs, CancellationToken ct = default(CancellationToken))
        {
            var tempTable = await CreateTempTableAsync<Product.Ref>(ct);
            await tempTable.Insert(refs).ExecuteAsync(ct);
            return await CreateQuery((DbQueryBuilder builder, Product.Lookup _) =>
            {
                builder.From(tempTable, out var t);
                var seqNo = t.Model.GetIdentity(true).Column;
                Debug.Assert(!ReferenceEquals(seqNo, null));
                builder.LeftJoin(Products, t.Key, out var p)
                    .AutoSelect().OrderBy(seqNo);
            }).ToDataSetAsync(ct);
        }
    }
}
