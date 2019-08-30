Imports System.Data
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

    Public Async Function LookupAsync(refs As DataSet(Of Product.Ref), Optional ct As CancellationToken = Nothing) As Task(Of DataSet(Of Product.Lookup))
        Dim tempTable = Await CreateTempTableAsync(Of Product.Ref)(ct)
        Await tempTable.InsertAsync(refs, ct)
        Return Await CreateQuery(
            Sub(builder As DbQueryBuilder, x As Product.Lookup)
                Dim t As Product.Ref = Nothing
                builder.From(tempTable, t)
                Dim seqNo = t.GetModel().GetIdentity(True).Column
                Debug.Assert(seqNo IsNot Nothing)
                Dim p As Product = Nothing
                builder.LeftJoin(Product, t.ForeignKey, p).AutoSelect().OrderBy(seqNo)
            End Sub).ToDataSetAsync(ct)
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
End Class
