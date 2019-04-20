Imports System.Threading

Partial Class Db
    Public Function GetSalesOrderHeadersAsync(filterText As String, orderBy As IReadOnlyList(Of IColumnComparer), ct As CancellationToken) As Task(Of DataSet(Of SalesOrderHeader))
        Dim result As DbSet(Of SalesOrderHeader)

        If String.IsNullOrEmpty(filterText) Then
            result = SalesOrderHeader
        Else
            result = GetSalesOrderHeaders(filterText)
        End If

        If orderBy IsNot Nothing AndAlso orderBy.Count > 0 Then result = result.OrderBy(Function(x) GetOrderBy(x, orderBy))
        Return result.ToDataSetAsync(ct)
    End Function

    Private Function GetSalesOrderHeaders(filterText As _String) As DbSet(Of SalesOrderHeader)
        Return SalesOrderHeader.Where(Function(x) x.SalesOrderNumber.Contains(filterText) Or x.PurchaseOrderNumber.Contains(filterText))
    End Function

    Private Shared Function GetOrderBy(model As Model, orderBy As IReadOnlyList(Of IColumnComparer)) As ColumnSort()
        Debug.Assert(orderBy IsNot Nothing AndAlso orderBy.Count > 0)
        Dim result = New ColumnSort(orderBy.Count - 1) {}

        For i As Integer = 0 To orderBy.Count - 1
            Dim column = orderBy(i).GetColumn(model)
            Dim direction = orderBy(i).Direction
            result(i) = If(direction = SortDirection.Descending, column.Desc(), column.Asc())
        Next

        Return result
    End Function

    Public Async Function GetSalesOrderInfoAsync(salesOrderID As _Int32, Optional ct As CancellationToken = Nothing) As Task(Of DataSet(Of SalesOrderInfo))
        Dim result = CreateQuery(Sub(builder As DbQueryBuilder, x As SalesOrderInfo)
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

    Public Overloads Function UpdateAsync(salesOrders As DataSet(Of SalesOrder), ct As CancellationToken) As Task
        Return ExecuteTransactionAsync(Function() PerformUpdateAsync(salesOrders, ct))
    End Function

    Private Async Function PerformUpdateAsync(salesOrders As DataSet(Of SalesOrder), ct As CancellationToken) As Task
        Dim salesOrder As SalesOrder = ModelOf(salesOrders)
        ModelOf(salesOrders).ResetRowIdentifiers()
        Await SalesOrderHeader.UpdateAsync(salesOrders, ct)
        Await Me.SalesOrderDetail.DeleteAsync(salesOrders, Function(s, x) s.Match(x.FK_SalesOrderHeader), ct)
        Dim salesOrderDetails = salesOrders.Children(Function(x) x.SalesOrderDetails)
        ModelOf(salesOrderDetails).ResetRowIdentifiers()
        Await Me.SalesOrderDetail.InsertAsync(salesOrderDetails, ct)
    End Function

    Public Overloads Function InsertAsync(salesOrders As DataSet(Of SalesOrder), ct As CancellationToken) As Task
        Return ExecuteTransactionAsync(Function() PerformInsertAsync(salesOrders, ct))
    End Function

    Private Async Function PerformInsertAsync(salesOrders As DataSet(Of SalesOrder), ByVal ct As CancellationToken) As Task
        ModelOf(salesOrders).ResetRowIdentifiers()
        Await SalesOrderHeader.InsertAsync(salesOrders, True, ct)
        Dim salesOrderDetails = salesOrders.Children(Function(x) x.SalesOrderDetails)
        ModelOf(salesOrderDetails).ResetRowIdentifiers()
        Await Me.SalesOrderDetail.InsertAsync(salesOrderDetails, ct)
    End Function

    Public Async Function LookupAsync(refs As DataSet(Of Product.Ref), Optional ct As CancellationToken = Nothing) As Task(Of DataSet(Of Product.Lookup))
        Dim tempTable = Await CreateTempTableAsync(Of Product.Ref)(ct)
        Await tempTable.InsertAsync(refs, ct)
        Return Await CreateQuery(Sub(builder As DbQueryBuilder, x As Product.Lookup)
                                     Dim t As Product.Ref = Nothing
                                     builder.From(tempTable, t)
                                     Dim seqNo = t.GetModel().GetIdentity(True).Column
                                     Debug.Assert(seqNo IsNot Nothing)
                                     Dim p As Product = Nothing
                                     builder.LeftJoin(Product, t.ForeignKey, p).AutoSelect().OrderBy(seqNo)
                                 End Sub).ToDataSetAsync(ct)
    End Function

    Public Function DeleteSalesOrderAsync(dataSet As DataSet(Of SalesOrderHeader.Key), ct As CancellationToken) As Task(Of Integer)
        Return SalesOrderHeader.DeleteAsync(dataSet, Function(s, x) s.Match(x), ct)
    End Function

    Public Function GetAddressLookupAsync(customerID As _Int32, ct As CancellationToken) As Task(Of DataSet(Of Address))
        Dim result = CreateQuery(Of Address)(
        Sub(builder, x)
            Dim ca As CustomerAddress = Nothing
            Dim a As Address = Nothing
            builder.From(CustomerAddress.Where(Function(y) y.CustomerID = customerID), ca) _
            .InnerJoin(Address, ca.FK_Address, a) _
            .AutoSelect()
        End Sub)
        Return result.ToDataSetAsync(ct)
    End Function

    Public Async Function UpdateSalesOrderAsync(salesOrders As DataSet(Of SalesOrderInfo), ct As CancellationToken) As Task
        ModelOf(salesOrders).ResetRowIdentifiers()
        Await SalesOrderHeader.UpdateAsync(salesOrders, ct)
        Await SalesOrderDetail.DeleteAsync(salesOrders, Function(s, x) s.Match(x.FK_SalesOrderHeader), ct)
        Dim salesOrderDetails = salesOrders.Children(Function(x) x.SalesOrderDetails)
        ModelOf(salesOrderDetails).ResetRowIdentifiers()
        Await SalesOrderDetail.InsertAsync(salesOrderDetails, ct)
    End Function

    Public Async Function CreateSalesOrderAsync(salesOrders As DataSet(Of SalesOrderInfo), ct As CancellationToken) As Task(Of Integer?)
        ModelOf(salesOrders).ResetRowIdentifiers()
        Await SalesOrderHeader.InsertAsync(salesOrders, True, ct)
        Dim salesOrderDetails = salesOrders.Children(Function(x) x.SalesOrderDetails)
        ModelOf(salesOrderDetails).ResetRowIdentifiers()
        Await SalesOrderDetail.InsertAsync(salesOrderDetails, ct)
        If salesOrders.Count > 0 Then
            Return ModelOf(salesOrders).SalesOrderID(0)
        Else
            Return Nothing
        End If
    End Function
End Class
