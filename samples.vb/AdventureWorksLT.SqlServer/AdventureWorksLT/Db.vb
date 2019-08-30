Imports System.Data.SqlClient

Partial Public Class Db
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

    Private m_Address As DbTable(Of Address)
    <DbTable("[SalesLT].[Address]", Description:="Street address information for customers.")>
    Public ReadOnly Property Address As DbTable(Of Address)
        Get
            Return GetTable(m_Address)
        End Get
    End Property

    Private m_Customer As DbTable(Of Customer)
    <DbTable("[SalesLT].[Customer]", Description:="Customer information.")>
    Public ReadOnly Property Customer As DbTable(Of Customer)
        Get
            Return GetTable(m_Customer)
        End Get
    End Property

    Private m_CustomerAddress As DbTable(Of CustomerAddress)
    <DbTable("[SalesLT].[CustomerAddress]", Description:="Cross-reference table mapping customers to their address(es).")>
    <Relationship(NameOf(FK_CustomerAddress_Customer_CustomerID), Description:="Foreign key constraint referencing Customer.CustomerID.")>
    <Relationship(NameOf(FK_CustomerAddress_Address_AddressID), Description:="Foreign key constraint referencing Address.AddressID.")>
    Public ReadOnly Property CustomerAddress As DbTable(Of CustomerAddress)
        Get
            Return GetTable(m_CustomerAddress)
        End Get
    End Property

    <_Relationship>
    Private Function FK_CustomerAddress_Customer_CustomerID(x As CustomerAddress) As KeyMapping
        Return x.FK_Customer.Join(Customer.Entity)
    End Function

    <_Relationship>
    Private Function FK_CustomerAddress_Address_AddressID(x As CustomerAddress) As KeyMapping
        Return x.FK_Address.Join(Address.Entity)
    End Function

    Private m_ProductCategory As DbTable(Of ProductCategory)
    <DbTable("[SalesLT].[ProductCategory]", Description:="High-level product categorization.")>
    <Relationship(NameOf(FK_ProductCategory_ProductCategory_ParentProductCategoryID_ProductCategoryID), Description:="Foreign key constraint referencing ProductCategory.ProductCategoryID.")>
    Public ReadOnly Property ProductCategory As DbTable(Of ProductCategory)
        Get
            Return GetTable(m_ProductCategory)
        End Get
    End Property

    <_Relationship>
    Private Function FK_ProductCategory_ProductCategory_ParentProductCategoryID_ProductCategoryID(x As ProductCategory) As KeyMapping
        Return x.FK_ParentProductCategory.Join(x)
    End Function

    Private m_ProductModel As DbTable(Of ProductModel)
    <DbTable("[SalesLT].[ProductModel]")>
    Public ReadOnly Property ProductModel As DbTable(Of ProductModel)
        Get
            Return GetTable(m_ProductModel)
        End Get
    End Property

    Private m_ProductDescription As DbTable(Of ProductDescription)
    <DbTable("[SalesLT].[ProductDescription]", Description:="Product descriptions in several languages.")>
    Public ReadOnly Property ProductDescription As DbTable(Of ProductDescription)
        Get
            Return GetTable(m_ProductDescription)
        End Get
    End Property

    Private m_ProductModelProductDescription As DbTable(Of ProductModelProductDescription)
    <DbTable("[SalesLT].[ProductModelProductDescription]", Description:="Cross-reference table mapping product descriptions and the language the description is written in.")>
    <Relationship(NameOf(FK_ProductModelProductDescription_ProductModel_ProductModelID), Description:="Foreign key constraint referencing ProductModel.ProductModelID.")>
    <Relationship(NameOf(FK_ProductModelProductDescription_ProductDescription_ProductDescriptionID), Description:="Foreign key constraint referencing ProductDescription.ProductDescriptionID.")>
    Public ReadOnly Property ProductModelProductDescription As DbTable(Of ProductModelProductDescription)
        Get
            Return GetTable(m_ProductModelProductDescription)
        End Get
    End Property

    <_Relationship>
    Private Function FK_ProductModelProductDescription_ProductModel_ProductModelID(x As ProductModelProductDescription) As KeyMapping
        Return x.FK_ProductModel.Join(ProductModel.Entity)
    End Function

    <_Relationship>
    Private Function FK_ProductModelProductDescription_ProductDescription_ProductDescriptionID(x As ProductModelProductDescription) As KeyMapping
        Return x.FK_ProductDescription.Join(ProductDescription.Entity)
    End Function

    Private m_Product As DbTable(Of Product)
    <DbTable("[SalesLT].[Product]", Description:="Products sold or used in the manfacturing of sold products.")>
    <Relationship(NameOf(FK_Product_ProductModel_ProductModelID))>
    <Relationship(NameOf(FK_Product_ProductCategory_ProductCategoryID))>
    Public ReadOnly Property Product As DbTable(Of Product)
        Get
            Return GetTable(m_Product)
        End Get
    End Property

    <_Relationship>
    Private Function FK_Product_ProductModel_ProductModelID(x As Product) As KeyMapping
        Return x.FK_ProductModel.Join(ProductModel.Entity)
    End Function

    <_Relationship>
    Private Function FK_Product_ProductCategory_ProductCategoryID(x As Product) As KeyMapping
        Return x.FK_ProductCategory.Join(ProductCategory.Entity)
    End Function

    Private m_SalesOrderHeader As DbTable(Of SalesOrderHeader)
    <DbTable("[SalesLT].[SalesOrderHeader]", Description:="General sales order information.")>
    <Relationship(NameOf(FK_SalesOrderHeader_Customer_CustomerID))>
    <Relationship(NameOf(FK_SalesOrderHeader_Address_BillTo_AddressID))>
    <Relationship(NameOf(FK_SalesOrderHeader_Address_ShipTo_AddressID))>
    Public ReadOnly Property SalesOrderHeader As DbTable(Of SalesOrderHeader)
        Get
            Return GetTable(m_SalesOrderHeader)
        End Get
    End Property

    <_Relationship>
    Private Function FK_SalesOrderHeader_Customer_CustomerID(x As SalesOrderHeader) As KeyMapping
        Return x.FK_Customer.Join(Customer.Entity)
    End Function

    <_Relationship>
    Private Function FK_SalesOrderHeader_Address_BillTo_AddressID(x As SalesOrderHeader) As KeyMapping
        Return x.FK_BillToCustomerAddress.Join(CustomerAddress.Entity)
    End Function

    <_Relationship>
    Private Function FK_SalesOrderHeader_Address_ShipTo_AddressID(x As SalesOrderHeader) As KeyMapping
        Return x.FK_ShipToCustomerAddress.Join(CustomerAddress.Entity)
    End Function

    Private m_SalesOrderDetail As DbTable(Of SalesOrderDetail)
    <DbTable("[SalesLT].[SalesOrderDetail]", Description:="Individual products associated with a specific sales order. See SalesOrderHeader.")>
    <Relationship(NameOf(FK_SalesOrderDetail_SalesOrderHeader))>
    <Relationship(NameOf(FK_SalesOrderDetail_Product))>
    Public ReadOnly Property SalesOrderDetail As DbTable(Of SalesOrderDetail)
        Get
            Return GetTable(m_SalesOrderDetail)
        End Get
    End Property

    <_Relationship>
    Private Function FK_SalesOrderDetail_SalesOrderHeader(x As SalesOrderDetail) As KeyMapping
        Return x.FK_SalesOrderHeader.Join(SalesOrderHeader.Entity)
    End Function

    <_Relationship>
    Private Function FK_SalesOrderDetail_Product(x As SalesOrderDetail) As KeyMapping
        Return x.FK_Product.Join(Product.Entity)
    End Function
End Class
