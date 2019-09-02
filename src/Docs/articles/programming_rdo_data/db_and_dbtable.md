# Db and DbTable

## Database Session

In RDO.Data, a database session is represented by a class which is derived from database session provider such as  <xref:DevZest.Data.SqlServer.SqlSession> or <xref:DevZest.Data.MySql.MySqlSession>. This class is normally named as `Db` by convention, with a connection string and/or database connection as constructor parameter:

# [C#](#tab/cs)

```cs
public partial class Db : SqlSession
{
    public Db(string connectionString, Action<Db> initializer = null)
        : base(CreateSqlConnection(connectionString))
    {
        initializer?.Invoke(this);
    }

    private static SqlConnection CreateSqlConnection(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentNullException(nameof(connectionString));
        return new SqlConnection(connectionString);
    }

    public Db(SqlConnection sqlConnection)
        : base(sqlConnection)
    {
    }
    ...
}
```

# [VB.Net](#tab/vb)

```vb
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

    Public Sub New(ByVal sqlConnection As SqlConnection)
        MyBase.New(sqlConnection)
    End Sub
    ...
End Class
```

***

The `Db` object is responsible to make connection and execute commands to the database server.

## Database Permanent Table

Database permanent table is readonly property of above `Db` class, with type of <xref:DevZest.Data.DbTable`1>. Optionally, foreign key relationship can be enforced by a pair of <xref:DevZest.Data.Annotations.RelationshipAttribute>/<xref:DevZest.Data.Annotations._RelationshipAttribute> and a implementation method. The following code defines two permanent tables SalesOrderHeader and SalesOrderDetail, and relationship between them:

# [C#](#tab/cs)

```cs
public partial class Db : ...
{
    ...
    private DbTable<SalesOrderHeader> _salesOrderHeader;
    public DbTable<SalesOrderHeader> SalesOrderHeader
    {
        get { return GetTable(ref _salesOrderHeader); }
    }

    private DbTable<SalesOrderDetail> _salesOrderDetail;
    [Relationship(nameof(FK_SalesOrderDetail_SalesOrderHeader))]
    public DbTable<SalesOrderDetail> SalesOrderDetail
    {
        get { return GetTable(ref _salesOrderDetail); }
    }

    [_Relationship]
    private KeyMapping FK_SalesOrderDetail_SalesOrderHeader(SalesOrderDetail _)
    {
        return _.FK_SalesOrderHeader.Join(SalesOrderHeader._);
    }
    ...
}
```

# [VB.Net](#tab/vb)

```vb
Partial Public Class Db
    ...
    Private m_SalesOrderHeader As DbTable(Of SalesOrderHeader)
    Public ReadOnly Property SalesOrderHeader As DbTable(Of SalesOrderHeader)
        Get
            Return GetTable(m_SalesOrderHeader)
        End Get
    End Property

    Private m_SalesOrderDetail As DbTable(Of SalesOrderDetail)
    <Relationship(NameOf(FK_SalesOrderDetail_SalesOrderHeader))>
    Public ReadOnly Property SalesOrderDetail As DbTable(Of SalesOrderDetail)
        Get
            Return GetTable(m_SalesOrderDetail)
        End Get
    End Property

    <_Relationship>
    Private Function FK_SalesOrderDetail_SalesOrderHeader(x As SalesOrderDetail) As KeyMapping
        Return x.FK_SalesOrderHeader.Join(SalesOrderHeader.Entity)
    End Function
    ...
End Class
```

***

The `Db` class and tables can be manipulated via Db Visualizer tool window. You can show Db Visualizer tool window by clicking menu "View" -> "Other Windows" -> "Db Visualizer" in Visual Studio:

![image](/images/db_visualizer.jpg)

In Db Visualizer tool window, click the left top ![image](/images/db_visualizer_add_table.jpg) button, the following dialog will be displayed:

![image](/images/db_visualizer_add_table_dialog.jpg)

Fill the dialog form and click "OK", code of DbTable definition will be generated automatically.

To add a foreign key, right click the source table in Db Visualizer to show the context menu:

![image](/images/db_visualizer_add_relationship.jpg)

Click context menu item "Add Relationship...", the following dialog will be displayed:

![image](/images/db_visualizer_add_relationship_dialog.jpg)

Fill the dialog form and click "OK", code of table relationship will be generated automatically.

## Database Temporary Table

A temporary table, as the name suggests, is a database table that exists temporarily on the database server during the life time of the session. Temporary tables are particularly useful when you have a large number of records in a table and you repeatedly need to interact with a small subset of those records. In such cases instead of filtering the data again and again to fetch the subset, you can filter the data once and store it in a temporary table. You can then execute your queries on that temporary table.

You can create temporary tables on-the-fly via Calling <xref:DevZest.Data.Primitives.DbSession.CreateTempTableAsync*> method of your `Db` class. Once created, you can use it as the same way as permanent tables. You don't need to delete the temporary tables, it will be deleted automatically by the underlying database session provider.
