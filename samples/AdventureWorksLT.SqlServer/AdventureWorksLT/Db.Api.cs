using DevZest.Data;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Samples.AdventureWorksLT
{
    partial class Db
    {
        public Task<DataSet<SalesOrderHeader>> GetSalesOrderHeadersAsync(string filterText, IReadOnlyList<IColumnComparer> orderBy, CancellationToken ct)
        {
            DbSet<SalesOrderHeader> result;
            if (string.IsNullOrEmpty(filterText))
                result = SalesOrderHeader;
            else
                result = GetSalesOrderHeaders(filterText);

            if (orderBy != null && orderBy.Count > 0)
                result = result.OrderBy(_ => GetOrderBy(_, orderBy));

            return result.ToDataSetAsync(ct);
        }

        private DbSet<SalesOrderHeader> GetSalesOrderHeaders(_String filterText)
        {
            return SalesOrderHeader.Where(_ => _.SalesOrderNumber.Contains(filterText) | _.PurchaseOrderNumber.Contains(filterText));
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

        public async Task<DataSet<Product.Lookup>> LookupAsync(DataSet<Product.Ref> refs, CancellationToken ct = default(CancellationToken))
        {
            var tempTable = await CreateTempTableAsync<Product.Ref>(ct);
            await tempTable.InsertAsync(refs, ct);
            return await CreateQuery((DbQueryBuilder builder, Product.Lookup _) =>
            {
                builder.From(tempTable, out var t);
                var seqNo = t.GetModel().GetIdentity(true).Column;
                Debug.Assert(!(seqNo is null));
                builder.LeftJoin(Product, t.ForeignKey, out var p)
                    .AutoSelect().OrderBy(seqNo);
            }).ToDataSetAsync(ct);
        }

        public Task<DataSet<Address>> GetAddressLookupAsync(_Int32 customerID, CancellationToken ct)
        {
            var result = CreateQuery<Address>((builder, _) =>
            {
                builder.From(CustomerAddress.Where(x => x.CustomerID == customerID), out var ca)
                    .InnerJoin(Address, ca.FK_Address, out var a)
                    .AutoSelect();
            });
            return result.ToDataSetAsync(ct);
        }

        public async Task UpdateAsync(DataSet<SalesOrder> salesOrders, CancellationToken ct)
        {
            using (var transaction = BeginTransaction())
            {
                await PerformUpdateAsync(salesOrders, ct);
                await transaction.CommitAsync(ct);
            }
        }

        private async Task PerformUpdateAsync(DataSet<SalesOrder> salesOrders, CancellationToken ct)
        {
            salesOrders._.ResetRowIdentifiers();
            await SalesOrderHeader.UpdateAsync(salesOrders, ct);
            await SalesOrderDetail.DeleteAsync(salesOrders, (s, _) => s.Match(_.FK_SalesOrderHeader), ct);
            var salesOrderDetails = salesOrders.GetChild(_ => _.SalesOrderDetails);
            salesOrderDetails._.ResetRowIdentifiers();
            await SalesOrderDetail.InsertAsync(salesOrderDetails, ct);
        }

        public async Task InsertAsync(DataSet<SalesOrder> salesOrders, CancellationToken ct)
        {
            using (var transaction = BeginTransaction())
            {
                await PerformInsertAsync(salesOrders, ct);
                await transaction.CommitAsync(ct);
            }
        }

        private async Task PerformInsertAsync(DataSet<SalesOrder> salesOrders, CancellationToken ct)
        {
            salesOrders._.ResetRowIdentifiers();
            await SalesOrderHeader.InsertAsync(salesOrders, true, ct);
            var salesOrderDetails = salesOrders.GetChild(_ => _.SalesOrderDetails);
            salesOrderDetails._.ResetRowIdentifiers();
            await SalesOrderDetail.InsertAsync(salesOrderDetails, ct);
        }

        #region SalesOrderCRUD

        private async Task EnsureConnectionOpenAsync(CancellationToken ct)
        {
            if (Connection.State != ConnectionState.Open)
                await OpenConnectionAsync(ct);
        }

        public async Task<DataSet<SalesOrderInfo>> GetSalesOrderInfoAsync(_Int32 salesOrderID, CancellationToken ct = default(CancellationToken))
        {
            var result = CreateQuery((DbQueryBuilder builder, SalesOrderInfo _) =>
            {
                builder.From(SalesOrderHeader, out var o)
                    .LeftJoin(Customer, o.FK_Customer, out var c)
                    .LeftJoin(Address, o.FK_ShipToAddress, out var shipTo)
                    .LeftJoin(Address, o.FK_BillToAddress, out var billTo)
                    .AutoSelect()
                    .AutoSelect(c, _.Customer)
                    .AutoSelect(shipTo, _.ShipToAddress)
                    .AutoSelect(billTo, _.BillToAddress)
                    .Where(o.SalesOrderID == salesOrderID);
            });

            await result.CreateChildAsync(_ => _.SalesOrderDetails, (DbQueryBuilder builder, SalesOrderInfoDetail _) =>
            {
                builder.From(SalesOrderDetail, out var d)
                    .LeftJoin(Product, d.FK_Product, out var p)
                    .AutoSelect()
                    .AutoSelect(p, _.Product)
                    .OrderBy(d.SalesOrderDetailID);
            }, ct);

            return await result.ToDataSetAsync(ct);
        }

        public async Task<int?> CreateSalesOrderAsync(DataSet<SalesOrderInfo> salesOrders, CancellationToken ct)
        {
            await EnsureConnectionOpenAsync(ct);
            using (var transaction = BeginTransaction())
            {
                salesOrders._.ResetRowIdentifiers();
                await SalesOrderHeader.InsertAsync(salesOrders, true, ct);
                var salesOrderDetails = salesOrders.GetChild(_ => _.SalesOrderDetails);
                salesOrderDetails._.ResetRowIdentifiers();
                await SalesOrderDetail.InsertAsync(salesOrderDetails, ct);

                await transaction.CommitAsync(ct);
                return salesOrders.Count > 0 ? salesOrders._.SalesOrderID[0] : null;
            }
        }

        public async Task UpdateSalesOrderAsync(DataSet<SalesOrderInfo> salesOrders, CancellationToken ct)
        {
            await EnsureConnectionOpenAsync(ct);
            using (var transaction = BeginTransaction())
            {
                salesOrders._.ResetRowIdentifiers();
                await SalesOrderHeader.UpdateAsync(salesOrders, ct);
                await SalesOrderDetail.DeleteAsync(salesOrders, (s, _) => s.Match(_.FK_SalesOrderHeader), ct);
                var salesOrderDetails = salesOrders.GetChild(_ => _.SalesOrderDetails);
                salesOrderDetails._.ResetRowIdentifiers();
                await SalesOrderDetail.InsertAsync(salesOrderDetails, ct);

                await transaction.CommitAsync(ct);
            }
        }

        public Task<int> DeleteSalesOrderAsync(DataSet<SalesOrderHeader.Key> dataSet, CancellationToken ct)
        {
            return SalesOrderHeader.DeleteAsync(dataSet, (s, _) => s.Match(_), ct);
        }

        #endregion
    }
}
