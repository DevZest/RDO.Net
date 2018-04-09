using DevZest.Data;
using DevZest.Samples.AdventureWorksLT;
using System;
using System.Collections.Generic;
using System.Data;
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

        public static async Task DeleteAsync(DataSet<SalesOrderHeader.Ref> dataSet, CancellationToken ct)
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
                    var ext = _.GetExtender<SalesOrderInfo.Ext>();
                    Debug.Assert(ext != null);
                    builder.From(db.SalesOrderHeaders, out var o)
                        .LeftJoin(db.Customers, o.Customer, out var c)
                        .LeftJoin(db.Addresses, o.ShipToAddress, out var shipTo)
                        .LeftJoin(db.Addresses, o.BillToAddress, out var billTo)
                        .AutoSelect()
                        .AutoSelect(shipTo, ext.ShipToAddress)
                        .AutoSelect(billTo, ext.BillToAddress)
                        .Where(o.SalesOrderID == _Int32.Param(salesOrderID));
                });

                result.CreateChild(_ => _.SalesOrderDetails, (DbQueryBuilder builder, SalesOrderDetail _) =>
                {
                    Debug.Assert(_.GetExtender<SalesOrderInfo.DetailExt>() != null);
                    builder.From(db.SalesOrderDetails, out var d)
                        .LeftJoin(db.Products, d.Product, out var p)
                        .AutoSelect();
                });

                return await result.ToDataSetAsync(ct);
            }
        }

        public static async Task<DataSet<Customer>> GetCustomerLookup(CancellationToken ct)
        {
            using (var db = await new Db(App.ConnectionString).OpenAsync(ct))
            {
                return await db.Customers.ToDataSetAsync(CustomerContactPerson.Initializer, ct);
            }
        }

        public static async Task<DataSet<Address>> GetAddressLookup(int customerID, CancellationToken ct)
        {
            using (var db = await new Db(App.ConnectionString).OpenAsync(ct))
            {
                var result = db.CreateQuery<Address>((builder, _) =>
                {
                    CustomerAddress ca;
                    Address a;
                    builder.From(db.CustomerAddresses.Where(db.CustomerAddresses._.CustomerID == customerID), out ca)
                        .InnerJoin(db.Addresses, ca.Address, out a)
                        .AutoSelect();
                });
                return await result.ToDataSetAsync(ct);
            }
        }

        public static async Task<DataSet<Product>> GetProductLookup(CancellationToken ct)
        {
            using (var db = await new Db(App.ConnectionString).OpenAsync(ct))
            {
                return await db.Products.ToDataSetAsync(ct);
            }
        }

        public static async Task UpdateSalesOrder<T>(DataSet<T> salesOrders, CancellationToken ct)
            where T : SalesOrder, new()
        {
            using (var db = await new Db(App.ConnectionString).OpenAsync(ct))
            {
                salesOrders._.ResetRowIdentifiers();
                await db.SalesOrderHeaders.Update(salesOrders).ExecuteAsync(ct);
                await db.SalesOrderDetails.Delete(salesOrders, (s, _) => s.Match(_.SalesOrderHeader)).ExecuteAsync(ct);
                var salesOrderDetails = salesOrders.Children(_ => _.SalesOrderDetails);
                salesOrderDetails._.ResetRowIdentifiers();
                await db.SalesOrderDetails.Insert(salesOrderDetails).ExecuteAsync(ct);
            }
        }

        public static async Task<int?> CreateSalesOrder<T>(DataSet<T> salesOrders, CancellationToken ct)
            where T : SalesOrder, new()
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
    }
}
