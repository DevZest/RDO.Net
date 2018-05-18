Imports DevZest.Data
Imports DevZest.Data.SqlServer
Imports System.ComponentModel
Imports System.Data.SqlClient
Imports System.Threading

Namespace DevZest.Samples.AdventureWorksLT
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
            SqlVersion = sqlVersion
        End Sub

        Public Sub New(ByVal sqlConnection As SqlConnection)
            MyBase.New(sqlConnection)
        End Sub

        Private m_Addresses As DbTable(Of Address)
        <Description("Street address information for customers.")>
        Public ReadOnly Property Addresses As DbTable(Of Address)
            Get
                Return GetTable(m_Addresses, "[SalesLT].[Address]")
            End Get
        End Property

        Private m_Customers As DbTable(Of Customer)
        <Description("Customer information.")>
        Public ReadOnly Property Customers As DbTable(Of Customer)
            Get
                Return GetTable(m_Customers, "[SalesLT].[Customer]")
            End Get
        End Property

        Private m_CustomerAddresses As DbTable(Of CustomerAddress)
        <Description("Cross-reference table mapping customers to their address(es).")>
        Public ReadOnly Property CustomerAddresses As DbTable(Of CustomerAddress)
            Get
                Return GetTable(m_CustomerAddresses, "[SalesLT].[CustomerAddress]",
                                Function(x) DbForeignKey("FK_CustomerAddress_Customer_CustomerID", "Foreign key constraint referencing Customer.CustomerID.",
                                                         x.FK_Customer, ModelOf(Customers), ForeignKeyAction.NoAction, ForeignKeyAction.NoAction),
                                Function(x) DbForeignKey("FK_CustomerAddress_Address_AddressID", "Foreign key constraint referencing Address.AddressID.",
                                                         x.FK_Address, ModelOf(Addresses), ForeignKeyAction.NoAction, ForeignKeyAction.NoAction))
            End Get
        End Property

        Private m_ProductCategories As DbTable(Of ProductCategory)
        <Description("High-level product categorization.")>
        Public ReadOnly Property ProductCategories As DbTable(Of ProductCategory)
            Get
                Return GetTable(m_ProductCategories, "[SalesLT].[ProductCategory]",
                    Function(x) DbForeignKey("FK_ProductCategory_ProductCategory_ParentProductCategoryID_ProductCategoryID",
                                             "Foreign key constraint referencing ProductCategory.ProductCategoryID.",
                                             x.FK_ParentProductCategory, x, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction))
            End Get
        End Property

        Private m_ProductModels As DbTable(Of ProductModel)
        Public ReadOnly Property ProductModels As DbTable(Of ProductModel)
            Get
                Return GetTable(m_ProductModels, "[SalesLT].[ProductModel]")
            End Get
        End Property

        Private m_ProductDescriptions As DbTable(Of ProductDescription)
        <Description("Product descriptions in several languages.")>
        Public ReadOnly Property ProductDescriptions As DbTable(Of ProductDescription)
            Get
                Return GetTable(m_ProductDescriptions, "[SalesLT].[ProductDescription]")
            End Get
        End Property

        Private m_ProductModelProductDescriptions As DbTable(Of ProductModelProductDescription)
        <Description("Cross-reference table mapping product descriptions and the language the description is written in.")>
        Public ReadOnly Property ProductModelProductDescriptions As DbTable(Of ProductModelProductDescription)
            Get
                Return GetTable(m_ProductModelProductDescriptions, "[SalesLT].[ProductModelProductDescription]",
                                Function(x) DbForeignKey("FK_ProductModelProductDescription_ProductModel_ProductModelID",
                                                         "Foreign key constraint referencing ProductModel.ProductModelID.",
                                                         x.FK_ProductModel, ModelOf(ProductModels), ForeignKeyAction.NoAction, ForeignKeyAction.NoAction),
                                Function(x) DbForeignKey("FK_ProductModelProductDescription_ProductDescription_ProductDescriptionID",
                                                         "Foreign key constraint referencing ProductDescription.ProductDescriptionID.",
                                                         x.FK_ProductDescription, ModelOf(ProductDescriptions), ForeignKeyAction.NoAction, ForeignKeyAction.NoAction))
            End Get
        End Property

        Private m_Products As DbTable(Of Product)
        <Description("Products sold or used in the manfacturing of sold products.")>
        Public ReadOnly Property Products As DbTable(Of Product)
            Get
                Return GetTable(m_Products, "[SalesLT].[Product]",
                                Function(x) DbForeignKey("FK_Product_ProductModel_ProductModelID", Nothing,
                                                         x.FK_ProductModel, ModelOf(ProductModels), ForeignKeyAction.NoAction, ForeignKeyAction.NoAction),
                                Function(x) DbForeignKey("FK_Product_ProductCategory_ProductCategoryID", Nothing,
                                                         x.FK_ProductCategory, ModelOf(ProductCategories), ForeignKeyAction.NoAction, ForeignKeyAction.NoAction))
            End Get
        End Property

        Private m_SalesOrderHeaders As DbTable(Of SalesOrderHeader)
        <Description("General sales order information.")>
        Public ReadOnly Property SalesOrderHeaders As DbTable(Of SalesOrderHeader)
            Get
                Return GetTable(m_SalesOrderHeaders, "[SalesLT].[SalesOrderHeader]",
                                Function(x) DbForeignKey("FK_SalesOrderHeader_Customer_CustomerID", Nothing,
                                                         x.FK_Customer, ModelOf(Customers), ForeignKeyAction.NoAction, ForeignKeyAction.NoAction),
                                Function(x) DbForeignKey("FK_SalesOrderHeader_Address_BillTo_AddressID", Nothing,
                                                         x.FK_BillToCustomerAddress, ModelOf(CustomerAddresses), ForeignKeyAction.NoAction, ForeignKeyAction.NoAction),
                                Function(x) DbForeignKey("FK_SalesOrderHeader_Address_ShipTo_AddressID", Nothing,
                                                         x.FK_ShipToCustomerAddress, ModelOf(CustomerAddresses), ForeignKeyAction.NoAction, ForeignKeyAction.NoAction))
            End Get
        End Property

        Private m_SalesOrderDetails As DbTable(Of SalesOrderDetail)
        <Description("Individual products associated with a specific sales order. See SalesOrderHeader.")>
        Public ReadOnly Property SalesOrderDetails As DbTable(Of SalesOrderDetail)
            Get
                Return GetTable(m_SalesOrderDetails, "[SalesLT].[SalesOrderDetail]",
                                Function(x) DbForeignKey(Nothing, Nothing,
                                                         x.FK_SalesOrderHeader, ModelOf(SalesOrderHeaders), ForeignKeyAction.NoAction, ForeignKeyAction.NoAction),
                                Function(x) DbForeignKey(Nothing, Nothing, x.FK_Product, ModelOf(Products), ForeignKeyAction.NoAction, ForeignKeyAction.NoAction))
            End Get
        End Property

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
                                         Dim ext = x.GetExtraColumns(Of SalesOrderInfo.Ext)()
                                         Debug.Assert(ext IsNot Nothing)
                                         Dim o As SalesOrderHeader = Nothing, c As Customer = Nothing, shipTo As Address = Nothing, billTo As Address = Nothing
                                         builder.From(SalesOrderHeaders, o).
                                         LeftJoin(Customers, o.FK_Customer, c).
                                         LeftJoin(Addresses, o.FK_ShipToAddress, shipTo).
                                         LeftJoin(Addresses, o.FK_BillToAddress, billTo).
                                         AutoSelect().
                                         AutoSelect(shipTo, ext.ShipToAddress).
                                         AutoSelect(billTo, ext.BillToAddress).
                                         Where(o.SalesOrderID = _Int32.Param(salesOrderID))
                                     End Sub)
            Await result.CreateChildAsync(Function(x) x.SalesOrderDetails,
                                          Sub(builder As DbQueryBuilder, x As SalesOrderInfoDetail)
                                              Debug.Assert(x.GetExtraColumns(Of Product.Lookup)() IsNot Nothing)
                                              Dim d As SalesOrderDetail = Nothing, p As Product = Nothing
                                              builder.From(SalesOrderDetails, d).LeftJoin(Products, d.FK_Product, p).AutoSelect()
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
                                         builder.LeftJoin(Products, t.Key, p).AutoSelect().OrderBy(seqNo)
                                     End Sub).ToDataSetAsync(ct)
        End Function
    End Class
End Namespace
