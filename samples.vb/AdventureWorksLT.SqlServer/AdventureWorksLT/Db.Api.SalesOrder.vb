Imports System.Data
Imports System.Threading

Partial Class Db
    Private Async Function EnsureConnectionOpenAsync(ct As CancellationToken) As Task
        If Connection.State <> ConnectionState.Open Then
            Await OpenConnectionAsync(ct)
        End If
    End Function

    Public Async Function GetSalesOrderInfoAsync(salesOrderID As _Int32, Optional ct As CancellationToken = Nothing) As Task(Of DataSet(Of SalesOrderInfo))
        Dim result = CreateQuery(
            Sub(builder As DbQueryBuilder, x As SalesOrderInfo)
                Dim o As SalesOrderHeader = Nothing, c As Customer = Nothing, shipTo As Address = Nothing, billTo As Address = Nothing
                builder.From(SalesOrderHeader, o).
                    LeftJoin(Customer, o.FK_Customer, c).
                    LeftJoin(Address, o.FK_ShipToAddress, shipTo).
                    LeftJoin(Address, o.FK_BillToAddress, billTo).
                    AutoSelect().
                    AutoSelect(c, x.Customer).
                    AutoSelect(shipTo, x.ShipToAddress).
                    AutoSelect(billTo, x.BillToAddress).
                    Where(o.SalesOrderID = salesOrderID)
            End Sub)
        Await result.CreateChildAsync(Function(x) x.SalesOrderDetails,
            Sub(builder As DbQueryBuilder, x As SalesOrderInfoDetail)
                Dim d As SalesOrderDetail = Nothing, p As Product = Nothing
                builder.From(SalesOrderDetail, d).LeftJoin(Product, d.FK_Product, p).AutoSelect().AutoSelect(p, x.Product)
            End Sub, ct)
        Return Await result.ToDataSetAsync(ct)
    End Function

    Public Function DeleteSalesOrderAsync(dataSet As DataSet(Of SalesOrderHeader.Key), ct As CancellationToken) As Task(Of Integer)
        Return SalesOrderHeader.DeleteAsync(dataSet, Function(s, x) s.Match(x), ct)
    End Function

    Public Async Function UpdateSalesOrderAsync(salesOrders As DataSet(Of SalesOrderInfo), ct As CancellationToken) As Task
        Await EnsureConnectionOpenAsync(ct)
        Using transaction = BeginTransaction()
            salesOrders.Entity.ResetRowIdentifiers()
            Await SalesOrderHeader.UpdateAsync(salesOrders, ct)
            Await SalesOrderDetail.DeleteAsync(salesOrders, Function(s, x) s.Match(x.FK_SalesOrderHeader), ct)
            Dim salesOrderDetails = salesOrders.GetChild(Function(x) x.SalesOrderDetails)
            salesOrderDetails.Entity.ResetRowIdentifiers()
            Await SalesOrderDetail.InsertAsync(salesOrderDetails, ct)
            Await transaction.CommitAsync(ct)
        End Using
    End Function

    Public Async Function CreateSalesOrderAsync(salesOrders As DataSet(Of SalesOrderInfo), ct As CancellationToken) As Task(Of Integer?)
        Await EnsureConnectionOpenAsync(ct)
        Using transaction = BeginTransaction()

            salesOrders.Entity.ResetRowIdentifiers()
            Await SalesOrderHeader.InsertAsync(salesOrders, True, ct)
            Dim salesOrderDetails = salesOrders.GetChild(Function(x) x.SalesOrderDetails)
            salesOrderDetails.Entity.ResetRowIdentifiers()
            Await SalesOrderDetail.InsertAsync(salesOrderDetails, ct)
            Await transaction.CommitAsync(ct)
            If salesOrders.Count > 0 Then
                Return salesOrders.Entity.SalesOrderID(0)
            Else
                Return Nothing
            End If
        End Using
    End Function
End Class
