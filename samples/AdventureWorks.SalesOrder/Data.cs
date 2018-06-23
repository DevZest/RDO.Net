using DevZest.Data;
using DevZest.Samples.AdventureWorksLT;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AdventureWorks.SalesOrders
{
    static class Data
    {
        public static async Task<DataSet<SalesOrderHeader>> GetSalesOrderHeadersAsync(string filterText, IReadOnlyList<IColumnComparer> orderBy, CancellationToken ct)
        {
            using (var db = await new Db(App.ConnectionString).OpenAsync(ct))
            {
                return await db.GetSalesOrderHeaders(filterText, orderBy).ToDataSetAsync(ct);
            }
        }

        public static async Task DeleteAsync(DataSet<SalesOrderHeader.Key> dataSet, CancellationToken ct)
        {
            using (var db = await new Db(App.ConnectionString).OpenAsync(ct))
            {
                await db.SalesOrderHeaders.Delete(dataSet, (s, _) => s.Match(_)).ExecuteAsync(ct);
            }
        }

        public static async Task<DataSet<SalesOrderInfo>> GetSalesOrderInfoAsync(int salesOrderID, CancellationToken ct)
        {
            using (var db = await new Db(App.ConnectionString).OpenAsync(ct))
            {
                var result = db.CreateQuery((DbQueryBuilder builder, SalesOrderInfo _) =>
                {
                    builder.From(db.SalesOrderHeaders, out var o)
                        .LeftJoin(db.Customers, o.FK_Customer, out var c)
                        .LeftJoin(db.Addresses, o.FK_ShipToAddress, out var shipTo)
                        .LeftJoin(db.Addresses, o.FK_BillToAddress, out var billTo)
                        .AutoSelect()
                        .AutoSelect(c, _.Customer)
                        .AutoSelect(shipTo, _.ShipToAddress)
                        .AutoSelect(billTo, _.BillToAddress)
                        .Where(o.SalesOrderID == _Int32.Param(salesOrderID));
                });

                await result.CreateChildAsync(_ => _.SalesOrderDetails, (DbQueryBuilder builder, SalesOrderInfoDetail _) =>
                {
                    builder.From(db.SalesOrderDetails, out var d)
                        .LeftJoin(db.Products, d.FK_Product, out var p)
                        .AutoSelect()
                        .AutoSelect(p, _.Product);
                }, ct);

                return await result.ToDataSetAsync(ct);
            }
        }

        public static async Task<DataSet<Customer>> GetCustomerLookupAsync(CancellationToken ct)
        {
            using (var db = await new Db(App.ConnectionString).OpenAsync(ct))
            {
                return await db.Customers.ToDataSetAsync(ct);
            }
        }

        public static async Task<DataSet<Address>> GetAddressLookupAsync(int customerID, CancellationToken ct)
        {
            using (var db = await new Db(App.ConnectionString).OpenAsync(ct))
            {
                var result = db.CreateQuery<Address>((builder, _) =>
                {
                    builder.From(db.CustomerAddresses.Where(db.CustomerAddresses._.CustomerID == customerID), out var ca)
                        .InnerJoin(db.Addresses, ca.FK_Address, out var a)
                        .AutoSelect();
                });
                return await result.ToDataSetAsync(ct);
            }
        }

        public static async Task<DataSet<Product>> GetProductLookupAsync(CancellationToken ct)
        {
            using (var db = await new Db(App.ConnectionString).OpenAsync(ct))
            {
                return await db.Products.ToDataSetAsync(ct);
            }
        }

        public static async Task UpdateSalesOrderAsync(DataSet<SalesOrderInfo> salesOrders, CancellationToken ct)
        {
            using (var db = await new Db(App.ConnectionString).OpenAsync(ct))
            {
                salesOrders._.ResetRowIdentifiers();
                await db.SalesOrderHeaders.Update(salesOrders).ExecuteAsync(ct);
                await db.SalesOrderDetails.Delete(salesOrders, (s, _) => s.Match(_.FK_SalesOrderHeader)).ExecuteAsync(ct);
                var salesOrderDetails = salesOrders.Children(_ => _.SalesOrderDetails);
                salesOrderDetails._.ResetRowIdentifiers();
                await db.SalesOrderDetails.Insert(salesOrderDetails).ExecuteAsync(ct);
            }
        }

        public static async Task<int?> CreateSalesOrderAsync(DataSet<SalesOrderInfo> salesOrders, CancellationToken ct)
        {
            using (var db = await new Db(App.ConnectionString).OpenAsync(ct))
            {
                salesOrders._.ResetRowIdentifiers();
                await db.SalesOrderHeaders.Insert(salesOrders, updateIdentity: true).ExecuteAsync(ct);
                var salesOrderDetails = salesOrders.Children(_ => _.SalesOrderDetails);
                salesOrderDetails._.ResetRowIdentifiers();
                await db.SalesOrderDetails.Insert(salesOrderDetails).ExecuteAsync(ct);
                return salesOrders.Count > 0 ? salesOrders._.SalesOrderID[0] : null;
            }
        }

        public static async Task<DataSet<Product.Lookup>> LookupAsync(DataSet<Product.Ref> data, CancellationToken ct)
        {
            using (var db = await new Db(App.ConnectionString).OpenAsync(ct))
            {
                return await db.LookupAsync(data, ct);
            }
        }
    }
}
