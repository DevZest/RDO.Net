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
        public static async Task<DataSet<SalesOrderHeader>> GetListAsync(string filterText, IReadOnlyList<IColumnComparer> orderBy, CancellationToken ct)
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

        public static async Task<DataSet<SalesOrderInfo>> GetItemAsync(int salesOrderID, CancellationToken ct)
        {
            using (var db = await new Db(App.ConnectionString).OpenAsync(ct))
            {
                return await db.GetSalesOrderInfo(salesOrderID).ToDataSetAsync(ct);
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

        public static async Task Update(DataSet<SalesOrder> salesOrder, CancellationToken ct)
        {
            using (var db = await new Db(App.ConnectionString).OpenAsync(ct))
            {
                await db.UpdateAsync(salesOrder, ct);
            }
        }

        public static async Task Create(DataSet<SalesOrder> salesOrder, CancellationToken ct)
        {
            using (var db = await new Db(App.ConnectionString).OpenAsync(ct))
            {
                await db.InsertAsync(salesOrder, ct);
            }
        }
    }
}
