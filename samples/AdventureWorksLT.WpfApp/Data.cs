using DevZest.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Samples.AdventureWorksLT
{
    static class Data
    {
        private static Db Create()
        {
            return new Db(App.ConnectionString);
        }

        public static async Task ExecuteAsync(Func<Db, Task> func)
        {
            using (var db = Create())
            {
                await func(db);
            }
        }

        public static async Task<T> ExecuteAsync<T>(Func<Db, Task<T>> func)
        {
            using (var db = Create())
            {
                return await func(db);
            }
        }

        public static async Task DeleteAsync(DataSet<SalesOrderHeader.Key> dataSet, CancellationToken ct)
        {
            using (var db = new Db(App.ConnectionString))
            {
                await db.SalesOrderHeader.DeleteAsync(dataSet, (s, _) => s.Match(_), ct);
            }
        }

        public static async Task<DataSet<SalesOrderInfo>> GetSalesOrderInfoAsync(int salesOrderID, CancellationToken ct)
        {
            using (var db = new Db(App.ConnectionString))
            {
                var result = db.CreateQuery((DbQueryBuilder builder, SalesOrderInfo _) =>
                {
                    builder.From(db.SalesOrderHeader, out var o)
                        .LeftJoin(db.Customer, o.FK_Customer, out var c)
                        .LeftJoin(db.Address, o.FK_ShipToAddress, out var shipTo)
                        .LeftJoin(db.Address, o.FK_BillToAddress, out var billTo)
                        .AutoSelect()
                        .AutoSelect(c, _.Customer)
                        .AutoSelect(shipTo, _.ShipToAddress)
                        .AutoSelect(billTo, _.BillToAddress)
                        .Where(o.SalesOrderID == _Int32.Param(salesOrderID));
                });

                await result.CreateChildAsync(_ => _.SalesOrderDetails, (DbQueryBuilder builder, SalesOrderInfoDetail _) =>
                {
                    builder.From(db.SalesOrderDetail, out var d)
                        .LeftJoin(db.Product, d.FK_Product, out var p)
                        .AutoSelect()
                        .AutoSelect(p, _.Product);
                }, ct);

                return await result.ToDataSetAsync(ct);
            }
        }

        public static async Task<DataSet<Customer>> GetCustomerLookupAsync(CancellationToken ct)
        {
            using (var db = new Db(App.ConnectionString))
            {
                return await db.Customer.ToDataSetAsync(ct);
            }
        }

        public static async Task<DataSet<Address>> GetAddressLookupAsync(int customerID, CancellationToken ct)
        {
            using (var db = new Db(App.ConnectionString))
            {
                var result = db.CreateQuery<Address>((builder, _) =>
                {
                    builder.From(db.CustomerAddress.Where(db.CustomerAddress._.CustomerID == customerID), out var ca)
                        .InnerJoin(db.Address, ca.FK_Address, out var a)
                        .AutoSelect();
                });
                return await result.ToDataSetAsync(ct);
            }
        }

        public static async Task<DataSet<Product>> GetProductLookupAsync(CancellationToken ct)
        {
            using (var db = new Db(App.ConnectionString))
            {
                return await db.Product.ToDataSetAsync(ct);
            }
        }

        public static async Task UpdateSalesOrderAsync(DataSet<SalesOrderInfo> salesOrders, CancellationToken ct)
        {
            using (var db = new Db(App.ConnectionString))
            {
                salesOrders._.ResetRowIdentifiers();
                await db.SalesOrderHeader.UpdateAsync(salesOrders, ct);
                await db.SalesOrderDetail.DeleteAsync(salesOrders, (s, _) => s.Match(_.FK_SalesOrderHeader), ct);
                var salesOrderDetails = salesOrders.Children(_ => _.SalesOrderDetails);
                salesOrderDetails._.ResetRowIdentifiers();
                await db.SalesOrderDetail.InsertAsync(salesOrderDetails, ct);
            }
        }

        public static async Task<int?> CreateSalesOrderAsync(DataSet<SalesOrderInfo> salesOrders, CancellationToken ct)
        {
            using (var db = new Db(App.ConnectionString))
            {
                salesOrders._.ResetRowIdentifiers();
                await db.SalesOrderHeader.InsertAsync(salesOrders, true, ct);
                var salesOrderDetails = salesOrders.Children(_ => _.SalesOrderDetails);
                salesOrderDetails._.ResetRowIdentifiers();
                await db.SalesOrderDetail.InsertAsync(salesOrderDetails, ct);
                return salesOrders.Count > 0 ? salesOrders._.SalesOrderID[0] : null;
            }
        }

        public static async Task<DataSet<Product.Lookup>> LookupAsync(DataSet<Product.Ref> data, CancellationToken ct)
        {
            using (var db = new Db(App.ConnectionString))
            {
                return await db.LookupAsync(data, ct);
            }
        }
    }
}
