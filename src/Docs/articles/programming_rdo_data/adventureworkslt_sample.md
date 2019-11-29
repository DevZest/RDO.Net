# AdventureWorksLT Sample

The best way to learn RDO.Data is to learn by example. The [AdventureWorkLT sample](https://github.com/DevZest/AdventureWorksLT), is provided to demonstrate most of RDO.Net:

| Project | Description |
|---------|-------------|
| `AdventureWorksLT` | Data and Business Layer of the application. |
| `AdventureWorksLT.DbInit` | Database mocking and deployment. |
| `AdventureWorksLT.WpfApp` | UI Layer of the application in WPF. |
| `AdventureWorksLT.Test` | Data logic tests. |

It's a typical fully featured LOB application based on the well known `AdventureWorksLT` SQL Server sample database. The data and business layer (`AdventureWorksLT` project), consists of the following components:

## Business Models

The following are business models in `AdventureWorksLT` project:

| Models | Description |
|--------|-------------|
| `Address`, `Customer`, `CustomerAddress` | Many to many relationship. |
| `ProductCategory` | Recursive relationship. |
| `ProductModel`, `ProductDescription`, `Product` | Master data. |
| `SalesOrderHeader`, `SalesOrderDetail` | Parent-child relationship. |

These business models, derived from type <xref:DevZest.Data.Model> or <xref:DevZest.Data.Model`1>, are fundamental part of your application. Your business models consist of:

* Data columns. Computed data column is supported.
* Keys (primary key, reference, foreign key, etc.)
* Database constraint and validation logic.
* Database index.
* Projection: subset of the model.
* Child model to support hierarchical data.

You may want to go through the sample business models to see how the above features are implemented. It's highly recommended to use *Model Visualizer* tool window as a starting point.

## Database Tables and Relationships

The database tables, are defined in the `Db.cs` (`Db.vb` in Visual Basic). The `Db` class is derived from <xref:DevZest.Data.SqlServer.SqlSession>, and tables are exposed as properties of type <xref:DevZest.Data.DbTable`1>:

# [C#](#tab/cs)

```cs
public partial class Db : SqlSession
{
    ...
    private DbTable<Address> _address;
    [DbTable("[SalesLT].[Address]", Description = "Street address information for customers.")]
    public DbTable<Address> Address
    {
        get { return GetTable(ref _address); }
    }
    ...
}
```

# [VB.Net](#tab/vb)

```vb
Partial Public Class Db
    Inherits SqlSession

    ...
    Private m_Address As DbTable(Of Address)
    <DbTable("[SalesLT].[Address]", Description:="Street address information for customers.")>
    Public ReadOnly Property Address As DbTable(Of Address)
        Get
            Return GetTable(m_Address)
        End Get
    End Property
    ...
End Class
```

***

The relationships between tables are also defined in this file. It's highly recommended to use *Db Visualizer* tool window to manipulate database tables and relationships.

## Database Query and Update

Database query and update, are implemented as instance method of `Db` partial class, in `Db.Api.cs` (or `Db.Api.vb` if in Visual Basic):

* <xref:DevZest.Data.DbQuery`1> is used to represent a database query. Both <xref:DevZest.Data.DbQuery`1> and <xref:DevZest.Data.DbTable`1> are derived from <xref:DevZest.Data.DbSet`1>.
* <xref:DevZest.Data.DbSet`1> has <xref:DevZest.Data.DbSet`1.ToDataSetAsync*> method to fill data into <xref:DevZest.Data.DataSet`1>, a local storage of data.
* <xref:DevZest.Data.DbTable`1> has <xref:DevZest.Data.DbTable`1.InsertAsync*>, <xref:DevZest.Data.DbTable`1.DeleteAsync*> and <xref:DevZest.Data.DbTable`1.UpdateAsync*> methods to update data from database server. All operations are truly set based.

The above query and update operations will generate SQL Abstract Syntax Tree (AST) and further be translated into native SQL by database provider, with very little overhead. It has best balance of code maintainability and performance.

You can find tests for query and update operations in `AdventureWorksLT.Test` project.
