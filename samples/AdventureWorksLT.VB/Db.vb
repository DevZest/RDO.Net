Imports System.Data.SqlClient
Imports System.Threading

Public Class Db
    Inherits SqlSession

    Public Sub New(connectionString As String, Optional initializer As Action(Of Db) = Nothing)
        MyBase.New(CreateSqlConnection(connectionString))
        If initializer IsNot Nothing Then initializer(Me)
    End Sub

    Private Shared Function CreateSqlConnection(connectionString As String) As SqlConnection
        If String.IsNullOrEmpty(connectionString) Then Throw New ArgumentNullException(NameOf(connectionString))
        Return New SqlConnection(connectionString)
    End Function

    Public Sub New(ByVal sqlVersion As SqlVersion)
        MyBase.New(New SqlConnection())
        sqlVersion = sqlVersion
    End Sub

    Public Sub New(ByVal sqlConnection As SqlConnection)
        MyBase.New(sqlConnection)
    End Sub

    Private m_Addresses As DbTable(Of Address)
    <DbTable("[SalesLT].[Address]", Description:="Street address information for customers.")>
    Public ReadOnly Property Addresses As DbTable(Of Address)
        Get
            Return GetTable(m_Addresses)
        End Get
    End Property

    Private m_Customers As DbTable(Of Customer)
    <DbTable("[SalesLT].[Customer]", Description:="Customer information.")>
    Public ReadOnly Property Customers As DbTable(Of Customer)
        Get
            Return GetTable(m_Customers)
        End Get
    End Property

    Private m_CustomerAddresses As DbTable(Of CustomerAddress)
    <DbTable("[SalesLT].[CustomerAddress]", Description:="Cross-reference table mapping customers to their address(es).")>
    <ForeignKey(NameOf(FK_CustomerAddress_Customer_CustomerID), Description:="Foreign key constraint referencing Customer.CustomerID.")>
    <ForeignKey(NameOf(FK_CustomerAddress_Address_AddressID), Description:="Foreign key constraint referencing Address.AddressID.")>
    Public ReadOnly Property CustomerAddresses As DbTable(Of CustomerAddress)
        Get
            Return GetTable(m_CustomerAddresses)
        End Get
    End Property

    <_ForeignKey>
    Private Function FK_CustomerAddress_Customer_CustomerID(x As CustomerAddress) As KeyMapping
        Return x.FK_Customer.Join(ModelOf(Customers))
    End Function

    <_ForeignKey>
    Private Function FK_CustomerAddress_Address_AddressID(x As CustomerAddress) As KeyMapping
        Return x.FK_Address.Join(ModelOf(Addresses))
    End Function

    Private m_ProductCategories As DbTable(Of ProductCategory)
    <DbTable("[SalesLT].[ProductCategory]", Description:="High-level product categorization.")>
    <ForeignKey(NameOf(FK_ProductCategory_ProductCategory_ParentProductCategoryID_ProductCategoryID), Description:="Foreign key constraint referencing ProductCategory.ProductCategoryID.")>
    Public ReadOnly Property ProductCategories As DbTable(Of ProductCategory)
        Get
            Return GetTable(m_ProductCategories)
        End Get
    End Property

    <_ForeignKey>
    Private Function FK_ProductCategory_ProductCategory_ParentProductCategoryID_ProductCategoryID(x As ProductCategory) As KeyMapping
        Return x.FK_ParentProductCategory.Join(x)
    End Function

    Private m_ProductModels As DbTable(Of ProductModel)
    <DbTable("[SalesLT].[ProductModel]")>
    Public ReadOnly Property ProductModels As DbTable(Of ProductModel)
        Get
            Return GetTable(m_ProductModels)
        End Get
    End Property

    Private m_ProductDescriptions As DbTable(Of ProductDescription)
    <DbTable("[SalesLT].[ProductDescription]", Description:="Product descriptions in several languages.")>
    Public ReadOnly Property ProductDescriptions As DbTable(Of ProductDescription)
        Get
            Return GetTable(m_ProductDescriptions)
        End Get
    End Property

    Private m_ProductModelProductDescriptions As DbTable(Of ProductModelProductDescription)
    <DbTable("[SalesLT].[ProductModelProductDescription]", Description:="Cross-reference table mapping product descriptions and the language the description is written in.")>
    <ForeignKey(NameOf(FK_ProductModelProductDescription_ProductModel_ProductModelID), Description:="Foreign key constraint referencing ProductModel.ProductModelID.")>
    <ForeignKey(NameOf(FK_ProductModelProductDescription_ProductDescription_ProductDescriptionID), Description:="Foreign key constraint referencing ProductDescription.ProductDescriptionID.")>
    Public ReadOnly Property ProductModelProductDescriptions As DbTable(Of ProductModelProductDescription)
        Get
            Return GetTable(m_ProductModelProductDescriptions)
        End Get
    End Property

    <_ForeignKey>
    Private Function FK_ProductModelProductDescription_ProductModel_ProductModelID(x As ProductModelProductDescription) As KeyMapping
        Return x.FK_ProductModel.Join(ModelOf(ProductModels))
    End Function

    <_ForeignKey>
    Private Function FK_ProductModelProductDescription_ProductDescription_ProductDescriptionID(x As ProductModelProductDescription) As KeyMapping
        Return x.FK_ProductDescription.Join(ModelOf(ProductDescriptions))
    End Function

    Private m_Products As DbTable(Of Product)
    <DbTable("[SalesLT].[Product]", Description:="Products sold or used in the manfacturing of sold products.")>
    <ForeignKey(NameOf(FK_Product_ProductModel_ProductModelID))>
    <ForeignKey(NameOf(FK_Product_ProductCategory_ProductCategoryID))>
    Public ReadOnly Property Products As DbTable(Of Product)
        Get
            Return GetTable(m_Products)
        End Get
    End Property

    <_ForeignKey>
    Private Function FK_Product_ProductModel_ProductModelID(x As Product) As KeyMapping
        Return x.FK_ProductModel.Join(ModelOf(ProductModels))
    End Function

    <_ForeignKey>
    Private Function FK_Product_ProductCategory_ProductCategoryID(x As Product) As KeyMapping
        Return x.FK_ProductCategory.Join(ModelOf(ProductCategories))
    End Function

    Private m_SalesOrderHeaders As DbTable(Of SalesOrderHeader)
    <DbTable("[SalesLT].[SalesOrderHeader]", Description:="General sales order information.")>
    <ForeignKey(NameOf(FK_SalesOrderHeader_Customer_CustomerID))>
    <ForeignKey(NameOf(FK_SalesOrderHeader_Address_BillTo_AddressID))>
    <ForeignKey(NameOf(FK_SalesOrderHeader_Address_ShipTo_AddressID))>
    Public ReadOnly Property SalesOrderHeaders As DbTable(Of SalesOrderHeader)
        Get
            Return GetTable(m_SalesOrderHeaders)
        End Get
    End Property

    <_ForeignKey>
    Private Function FK_SalesOrderHeader_Customer_CustomerID(x As SalesOrderHeader) As KeyMapping
        Return x.FK_Customer.Join(ModelOf(Customers))
    End Function

    <_ForeignKey>
    Private Function FK_SalesOrderHeader_Address_BillTo_AddressID(x As SalesOrderHeader) As KeyMapping
        Return x.FK_BillToCustomerAddress.Join(ModelOf(CustomerAddresses))
    End Function

    <_ForeignKey>
    Private Function FK_SalesOrderHeader_Address_ShipTo_AddressID(x As SalesOrderHeader) As KeyMapping
        Return x.FK_ShipToCustomerAddress.Join(ModelOf(CustomerAddresses))
    End Function

    Private m_SalesOrderDetails As DbTable(Of SalesOrderDetail)
    <DbTable("[SalesLT].[SalesOrderDetail]", Description:="Individual products associated with a specific sales order. See SalesOrderHeader.")>
    <ForeignKey(NameOf(FK_SalesOrderDetail_SalesOrderHeader))>
    <ForeignKey(NameOf(FK_SalesOrderDetail_Product))>
    Public ReadOnly Property SalesOrderDetails As DbTable(Of SalesOrderDetail)
        Get
            Return GetTable(m_SalesOrderDetails)
        End Get
    End Property

    <_ForeignKey>
    Private Function FK_SalesOrderDetail_SalesOrderHeader(x As SalesOrderDetail) As KeyMapping
        Return x.FK_SalesOrderHeader.Join(ModelOf(SalesOrderHeaders))
    End Function

    <_ForeignKey>
    Private Function FK_SalesOrderDetail_Product(x As SalesOrderDetail) As KeyMapping
        Return x.FK_Product.Join(ModelOf(Products))
    End Function

    Public Function GetSalesOrderHeaders(filterText As String, orderBy As IReadOnlyList(Of IColumnComparer)) As DbSet(Of SalesOrderHeader)
        Dim result As DbSet(Of SalesOrderHeader)

        If String.IsNullOrEmpty(filterText) Then
            result = SalesOrderHeaders
        Else
            result = SalesOrderHeaders.Where(Function(__) __.SalesOrderNumber.Contains(filterText) Or __.PurchaseOrderNumber.Contains(filterText))
        End If

        If orderBy IsNot Nothing AndAlso orderBy.Count > 0 Then result = result.OrderBy(GetOrderBy(ModelOf(result), orderBy))
        Return result
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

    Public Async Function GetSalesOrderInfoAsync(salesOrderID As Integer, Optional ct As CancellationToken = Nothing) As Task(Of DbSet(Of SalesOrderInfo))
        Dim result = CreateQuery(Sub(builder As DbQueryBuilder, x As SalesOrderInfo)
                                     Dim o As SalesOrderHeader = Nothing, c As Customer = Nothing, shipTo As Address = Nothing, billTo As Address = Nothing
                                     builder.From(SalesOrderHeaders, o).
                                     LeftJoin(Customers, o.FK_Customer, c).
                                     LeftJoin(Addresses, o.FK_ShipToAddress, shipTo).
                                     LeftJoin(Addresses, o.FK_BillToAddress, billTo).
                                     AutoSelect().
                                     AutoSelect(c, x.Customer).
                                     AutoSelect(shipTo, x.ShipToAddress).
                                     AutoSelect(billTo, x.BillToAddress).
                                     Where(o.SalesOrderID = _Int32.Param(salesOrderID))
                                 End Sub)
        Await result.CreateChildAsync(Function(x) x.SalesOrderDetails,
                                      Sub(builder As DbQueryBuilder, x As SalesOrderInfoDetail)
                                          Dim d As SalesOrderDetail = Nothing, p As Product = Nothing
                                          builder.From(SalesOrderDetails, d).LeftJoin(Products, d.FK_Product, p).AutoSelect().AutoSelect(p, x.Product)
                                      End Sub, ct)
        Return result
    End Function

    Public Overloads Function UpdateAsync(salesOrders As DataSet(Of SalesOrder), ct As CancellationToken) As Task
        Return ExecuteTransactionAsync(Function() PerformUpdateAsync(salesOrders, ct))
    End Function

    Private Async Function PerformUpdateAsync(salesOrders As DataSet(Of SalesOrder), ct As CancellationToken) As Task
        Dim salesOrder As SalesOrder = ModelOf(salesOrders)
        ModelOf(salesOrders).ResetRowIdentifiers()
        Await SalesOrderHeaders.Update(salesOrders).ExecuteAsync(ct)
        Await Me.SalesOrderDetails.Delete(salesOrders, Function(s, x) s.Match(x.FK_SalesOrderHeader)).ExecuteAsync(ct)
        Dim salesOrderDetails = salesOrders.Children(Function(x) x.SalesOrderDetails)
        ModelOf(salesOrderDetails).ResetRowIdentifiers()
        Await Me.SalesOrderDetails.Insert(salesOrderDetails).ExecuteAsync(ct)
    End Function

    Public Overloads Function InsertAsync(salesOrders As DataSet(Of SalesOrder), ct As CancellationToken) As Task
        Return ExecuteTransactionAsync(Function() PerformInsertAsync(salesOrders, ct))
    End Function

    Private Async Function PerformInsertAsync(salesOrders As DataSet(Of SalesOrder), ByVal ct As CancellationToken) As Task
        ModelOf(salesOrders).ResetRowIdentifiers()
        Await SalesOrderHeaders.Insert(salesOrders, updateIdentity:=True).ExecuteAsync(ct)
        Dim salesOrderDetails = salesOrders.Children(Function(x) x.SalesOrderDetails)
        ModelOf(salesOrderDetails).ResetRowIdentifiers()
        Await Me.SalesOrderDetails.Insert(salesOrderDetails).ExecuteAsync(ct)
    End Function

    Public Async Function LookupAsync(refs As DataSet(Of Product.Ref), Optional ct As CancellationToken = Nothing) As Task(Of DataSet(Of Product.Lookup))
        Dim tempTable = Await CreateTempTableAsync(Of Product.Ref)(ct)
        Await tempTable.Insert(refs).ExecuteAsync(ct)
        Return Await CreateQuery(Sub(builder As DbQueryBuilder, x As Product.Lookup)
                                     Dim t As Product.Ref = Nothing
                                     builder.From(tempTable, t)
                                     Dim seqNo = t.Model.GetIdentity(True).Column
                                     Debug.Assert(seqNo IsNot Nothing)
                                     Dim p As Product = Nothing
                                     builder.LeftJoin(Products, t.ForeignKey, p).AutoSelect().OrderBy(seqNo)
                                 End Sub).ToDataSetAsync(ct)
    End Function
End Class
