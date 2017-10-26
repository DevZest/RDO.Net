using DevZest.Data;
using DevZest.Samples.AdventureWorksLT;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AdventureWorks.SalesOrders
{
    static class Data
    {
        public static async Task<DataSet<SalesOrder>> GetListAsync(string filterText, IReadOnlyList<IColumnComparer> orderBy, CancellationToken ct)
        {
            using (var db = await new Db(App.ConnectionString).OpenAsync(ct))
            {
                return await db.GetSalesOrderList(filterText, orderBy).ToDataSetAsync(ct);
            }
        }

        public static async Task DeleteAsync(DataSet<SalesOrder.Ref> dataSet, CancellationToken ct)
        {
            using (var db = await new Db(App.ConnectionString).OpenAsync(ct))
            {
                await db.SalesOrders.DeleteAsync(dataSet, _ => _.PrimaryKey, ct);
            }
        }

        public static async Task<DataSet<SalesOrderToEdit>> GetItemAsync(int salesOrderID, CancellationToken ct)
        {
            using (var db = await new Db(App.ConnectionString).OpenAsync(ct))
            {
                return await db.GetSalesOrderToEdit(salesOrderID).ToDataSetAsync(ct);
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
    }
}
