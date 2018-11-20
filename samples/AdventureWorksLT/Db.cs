using DevZest.Data;
using DevZest.Data.Annotations;
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
            initializer?.Invoke(this);
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
        [DbTable("[SalesLT].[Address]", Description = "Street address information for customers.")]
        public DbTable<Address> Addresses
        {
            get { return GetTable(ref _addresses); }
        }

        private DbTable<Customer> _customers;
        [DbTable("[SalesLT].[Customer]", Description = "Customer information.")]
        public DbTable<Customer> Customers
        {
            get { return GetTable(ref _customers); }
        }

        private DbTable<CustomerAddress> _customerAddresses;
        [DbTable("[SalesLT].[CustomerAddress]", Description = "Cross-reference table mapping customers to their address(es).")]
        [ForeignKey(nameof(FK_CustomerAddress_Customer_CustomerID), Description = "Foreign key constraint referencing Customer.CustomerID.")]
        [ForeignKey(nameof(FK_CustomerAddress_Address_AddressID), Description = "Foreign key constraint referencing Address.AddressID.")]
        public DbTable<CustomerAddress> CustomerAddresses
        {
            get { return GetTable(ref _customerAddresses); }
        }

        [_ForeignKey]
        private KeyMapping FK_CustomerAddress_Customer_CustomerID(CustomerAddress _)
        {
            return _.FK_Customer.Join(Customers._);
        }

        private KeyMapping FK_CustomerAddress_Address_AddressID(CustomerAddress _)
        {
            return _.FK_Address.Join(Addresses._);
        }

        private DbTable<ProductCategory> _productCategories;
        [DbTable("[SalesLT].[ProductCategory]", Description = "High-level product categorization.")]
        [ForeignKey(nameof(FK_ProductCategory_ProductCategory_ParentProductCategoryID_ProductCategoryID), Description = "Foreign key constraint referencing ProductCategory.ProductCategoryID.")]
        public DbTable<ProductCategory> ProductCategories
        {
            get { return GetTable(ref _productCategories); }
        }

        [_ForeignKey]
        private KeyMapping FK_ProductCategory_ProductCategory_ParentProductCategoryID_ProductCategoryID(ProductCategory _)
        {
            return _.FK_ParentProductCategory.Join(_);
        }

        private DbTable<ProductModel> _productModels;
        [DbTable("[SalesLT].[ProductModel]")]
        public DbTable<ProductModel> ProductModels
        {
            get { return GetTable(ref _productModels); }
        }

        private DbTable<ProductDescription> _productDescriptions;
        [DbTable("[SalesLT].[ProductDescription]", Description = "Product descriptions in several languages.")]
        public DbTable<ProductDescription> ProductDescriptions
        {
            get { return GetTable(ref _productDescriptions); }
        }

        private DbTable<ProductModelProductDescription> _productModelProductDescriptions;
        [DbTable("[SalesLT].[ProductModelProductDescription]", Description = "Cross-reference table mapping product descriptions and the language the description is written in.")]
        [ForeignKey(nameof(FK_ProductModelProductDescription_ProductModel_ProductModelID), Description = "Foreign key constraint referencing ProductModel.ProductModelID.")]
        [ForeignKey(nameof(FK_ProductModelProductDescription_ProductDescription_ProductDescriptionID), Description = "Foreign key constraint referencing ProductDescription.ProductDescriptionID.")]
        public DbTable<ProductModelProductDescription> ProductModelProductDescriptions
        {
            get { return GetTable(ref _productModelProductDescriptions); }
        }

        [_ForeignKey]
        private KeyMapping FK_ProductModelProductDescription_ProductModel_ProductModelID(ProductModelProductDescription _)
        {
            return _.FK_ProductModel.Join(ProductModels._);
        }

        [_ForeignKey]
        private KeyMapping FK_ProductModelProductDescription_ProductDescription_ProductDescriptionID(ProductModelProductDescription _)
        {
            return _.FK_ProductDescription.Join(ProductDescriptions._);
        }

        private DbTable<Product> _products;
        [DbTable("[SalesLT].[Product]", Description = "Products sold or used in the manfacturing of sold products.")]
        [ForeignKey(nameof(FK_Product_ProductModel_ProductModelID))]
        [ForeignKey(nameof(FK_Product_ProductCategory_ProductCategoryID))]
        public DbTable<Product> Products
        {
            get { return GetTable(ref _products); }
        }

        [_ForeignKey]
        private KeyMapping FK_Product_ProductModel_ProductModelID(Product _)
        {
            return _.FK_ProductModel.Join(ProductModels._);
        }

        [_ForeignKey]
        private KeyMapping FK_Product_ProductCategory_ProductCategoryID(Product _)
        {
            return _.FK_ProductCategory.Join(ProductCategories._);
        }

        private DbTable<SalesOrderHeader> _salesOrderHeaders;
        [DbTable("[SalesLT].[SalesOrderHeader]", Description = "General sales order information.")]
        [ForeignKey(nameof(FK_SalesOrderHeader_Customer_CustomerID))]
        [ForeignKey(nameof(FK_SalesOrderHeader_Address_BillTo_AddressID))]
        [ForeignKey(nameof(FK_SalesOrderHeader_Address_ShipTo_AddressID))]
        public DbTable<SalesOrderHeader> SalesOrderHeaders
        {
            get { return GetTable(ref _salesOrderHeaders); }
        }

        [_ForeignKey]
        private KeyMapping FK_SalesOrderHeader_Customer_CustomerID(SalesOrderHeader _)
        {
            return _.FK_Customer.Join(Customers._);
        }

        [_ForeignKey]
        private KeyMapping FK_SalesOrderHeader_Address_BillTo_AddressID(SalesOrderHeader _)
        {
            return _.FK_BillToCustomerAddress.Join(CustomerAddresses._);
        }

        [_ForeignKey]
        private KeyMapping FK_SalesOrderHeader_Address_ShipTo_AddressID(SalesOrderHeader _)
        {
            return _.FK_ShipToCustomerAddress.Join(CustomerAddresses._);
        }

        private DbTable<SalesOrderDetail> _salesOrderDetails;
        [DbTable("[SalesLT].[SalesOrderDetail]", Description = "Individual products associated with a specific sales order. See SalesOrderHeader.")]
        [ForeignKey(nameof(FK_SalesOrderDetail_SalesOrderHeader))]
        [ForeignKey(nameof(FK_SalesOrderDetail_Product))]
        public DbTable<SalesOrderDetail> SalesOrderDetails
        {
            get { return GetTable(ref _salesOrderDetails); }
        }

        [_ForeignKey]
        private KeyMapping FK_SalesOrderDetail_SalesOrderHeader(SalesOrderDetail _)
        {
            return _.FK_SalesOrderHeader.Join(SalesOrderHeaders._);
        }

        [_ForeignKey]
        private KeyMapping FK_SalesOrderDetail_Product(SalesOrderDetail _)
        {
            return _.FK_Product.Join(Products._);
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
                builder.From(SalesOrderHeaders, out var o)
                    .LeftJoin(Customers, o.FK_Customer, out var c)
                    .LeftJoin(Addresses, o.FK_ShipToAddress, out var shipTo)
                    .LeftJoin(Addresses, o.FK_BillToAddress, out var billTo)
                    .AutoSelect()
                    .AutoSelect(c, _.Customer)
                    .AutoSelect(shipTo, _.ShipToAddress)
                    .AutoSelect(billTo, _.BillToAddress)
                    .Where(o.SalesOrderID == _Int32.Param(salesOrderID));
            });

            await result.CreateChildAsync(_ => _.SalesOrderDetails, (DbQueryBuilder builder, SalesOrderInfoDetail _) =>
            {
                builder.From(SalesOrderDetails, out var d)
                    .LeftJoin(Products, d.FK_Product, out var p)
                    .AutoSelect()
                    .AutoSelect(p, _.Product);
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
                Debug.Assert(!(seqNo is null));
                builder.LeftJoin(Products, t.ForeignKey, out var p)
                    .AutoSelect().OrderBy(seqNo);
            }).ToDataSetAsync(ct);
        }
    }
}
