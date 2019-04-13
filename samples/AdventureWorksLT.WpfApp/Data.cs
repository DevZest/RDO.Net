using DevZest.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Samples.AdventureWorksLT
{
    static class Data
    {
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
