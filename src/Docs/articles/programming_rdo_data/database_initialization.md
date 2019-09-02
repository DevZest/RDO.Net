# Database Initialization

In RDO.Data, database creation and initialization are extensively supported. You can:

* Generate database with all tables, or subset of tables, to any specified destination, with or without initial or testing data;
* Generate DataSet (C# or VB code) from existing database so that you can use existing external database editing tool such as SQL Server Management Studio to edit initial or testing data, then save these data into DataSet source code.

You application should have a separate console application project to handle database creation and initialization. This project:

* By convention is named with a `.DbInit` suffix;
* References [DevZest.Data.DbInit](https://www.nuget.org/packages/DevZest.Data.DbInit/) nuget package;
* Call <xref:DevZest.Data.DbInit.DbInitExtensions.RunDbInit*> at entry point;
* Defines classes derived from <xref:DevZest.Data.DbInit.DbSessionProvider`1>, <xref:DevZest.Data.DbGenerator`1> or <xref:DevZest.Data.DbMock`1>.

RDO.Tools runs this console application project from Visual Studio, just like unit test runner runs your unit test project. For detailed information, please refer to `Tutorial` and `AdventureWorksLT` samples.

## Database Session Provider

Database session provider is your class derived from <xref:DevZest.Data.DbInit.DbSessionProvider`1>. It creates your `Db` object as the destination to create and initialize your database. The following is an example of database session provider:

# [C#](#tab/cs)

```cs
[EmptyDb]
public sealed class _DevDb : DbSessionProvider<Db>
{
    public override Db Create(string projectPath)
    {
        var dbFolder = Path.Combine(projectPath, @"LocalDb");
        string attachDbFilename = Path.Combine(dbFolder, "AdventureWorksLT.Design.mdf");
        File.Copy(Path.Combine(dbFolder, "EmptyDb.mdf"), attachDbFilename, true);
        File.Copy(Path.Combine(dbFolder, "EmptyDb_log.ldf"), Path.Combine(dbFolder, "AdventureWorksLT.Design_log.ldf"), true);
        var connectionString = string.Format(@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename);
        return new Db(connectionString);
    }
}

public sealed class DevDb : DbSessionProvider<Db>
{
    public override Db Create(string projectPath)
    {
        var dbFolder = Path.Combine(projectPath, @"LocalDb");
        string attachDbFilename = Path.Combine(dbFolder, "AdventureWorksLT.Design.mdf");
        var connectionString = string.Format(@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename);
        return new Db(connectionString);
    }
}
```

# [VB.Net](#tab/vb)

```vb
<EmptyDb>
Public Class _DevDb
    Inherits DbSessionProvider(Of Db)

    Public Overrides Function Create(projectPath As String) As Db
        Dim dbFolder = Path.Combine(projectPath, "LocalDb")
        Dim attachDbFilename = Path.Combine(dbFolder, "AdventureWorksLT.DbInit.mdf")
        File.Copy(Path.Combine(dbFolder, "EmptyDb.mdf"), attachDbFilename, True)
        File.Copy(Path.Combine(dbFolder, "EmptyDb_log.ldf"), Path.Combine(dbFolder, "AdventureWorksLT.DbInit_log.ldf"), True)
        Dim connectionString = String.Format("Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename)
        Return New Db(connectionString)
    End Function
End Class

Public Class DevDb
    Inherits DbSessionProvider(Of Db)

    Public Overrides Function Create(projectPath As String) As Db
        Dim dbFolder = Path.Combine(projectPath, "LocalDb")
        Dim attachDbFilename = Path.Combine(dbFolder, "AdventureWorksLT.DbInit.mdf")
        Dim connectionString = String.Format("Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename)
        Return New Db(connectionString)
    End Function
End Class
```

***

The above example code declares two destinations: `_DevDb` and `DevDb`. They both point to `LocalDb/AdventureWorksLT.DbInit.mdf` database, relative to current project folder. However, `_DevDb` is decorated with <xref:DevZest.Data.DbInit.EmptyDbAttribute> indicating it's an empty database, whereas `DevDb` is a existing database with tables created/initialized. The later one is used to generate DataSet from database.

You can also define database session provider for production environment:

```cs
[EmptyDb]
public class _PrdDb : DbSessionProvider<Db>
{
    public override Db Create(string projectPath)
    {
        var connectionString = string.Format($"Server=127.0.0.1,4000;Database=AdventureWorksLT;User Id=sa;Password={Password};");
        return new Db(connectionString);
    }

    [Input(IsPassword = true)]
    public string Password { get; set; }
}
```

Your source code should not contain any sensitive data such as database connection password. You can use <xref:DevZest.Data.DbInit.InputAttribute> to decorate a public settable string property so that the runner can prompt the user to enter the value, or read value from environment variable, when executed.

You can generate database from database session provider, with all tables and without initial data. In Visual Studio, right click your database session provider class source code which decorated with `EmptyDb` attribute (as `_DevDb` in previous example), to show the context menu:

# [C#](#tab/cs)

![image](/images/dbinit_generate_db_from_dbsessionprovider_cs.jpg)

# [VB.Net](#tab/vb)

![image](/images/dbinit_generate_db_from_dbsessionprovider_vb.jpg)

***

Click `Generate Db...` from context menu, the following dialog will be displayed:

![image](/images/dbinit_generate_db_from_dbsessionprovider_dialog.jpg)

Click `OK` button, the database will be created:

![image](/images/dbinit_generate_db_result.jpg)

## Database Initializer

A database initializer is a class that takes care of database creation and initialization. You can initialize your database with all tables via deriving your database initializer class from <xref:DevZest.Data.DbGenerator`1> class, or you can mock your database with subset of tables via deriving your database initializer class from <xref:DevZest.Data.DbMock`1> class:

* <xref:DevZest.Data.DbGenerator`1> is concrete class, can be used directly with database session providers. The preceding database generation example is using this class implicitly. You can optionally call <xref:DevZest.Data.DbGenerator`1.SetData*> in <xref:DevZest.Data.DbGenerator`1.InitializeData*> override.
* <xref:DevZest.Data.DbMock`1> is abstract class, must be derived to override <xref:DevZest.Data.Primitives.DbInitializer.Initialize*> to call <xref:DevZest.Data.DbMock`1.Mock*> to specify table(s) and/or data explicitly.

You can generate the database from your database initializer class by right clicking the source code in Visual Studio, as described in previous example.

Both <xref:DevZest.Data.DbGenerator`1.SetData*> and <xref:DevZest.Data.DbMock`1.Mock*> methods accept a delegate to provide initial/testing data. This delegate must be a static method which returns a DataSet:

# [C#](#tab/cs)

```cs
public sealed class MockSalesOrder : DbMock<Db>
{
    private static DataSet<SalesOrderHeader> Headers()
    {
        ...
    }

    private static DataSet<SalesOrderDetail> Details()
    {
        ...
    }

    protected override void Initialize()
    {
        // The order of mocking table does not matter, the dependencies will be sorted out automatically.
        Mock(Db.SalesOrderDetail, Details);
        Mock(Db.SalesOrderHeader, Headers);
    }
}
```

# [VB.Net](#tab/vb)

```vb
Public Class MockSalesOrder
    Inherits DbMock(Of Db)

    Private Shared Function Headers() As DataSet(Of SalesOrderHeader)
        ...
    End Function

    Private Shared Function Details() As DataSet(Of SalesOrderDetail)
        ...
    End Function

    Protected Overrides Sub Initialize()
        Mock(Db.SalesOrderHeader, AddressOf Headers)
        Mock(Db.SalesOrderDetail, AddressOf Details)
    End Sub
End Class
```

***

If your database initializer class contains any static method returns DataSet, right clicking the source code in Visual Studio will display `Generate DataSet(s)...` context menu:

![image](/images/dbinit_generate_dataset.jpg)

Click `Generate DataSet(s)...` context menu item, the following dialog will be displayed:

![image](/images/dbinit_generate_dataset_dialog.jpg)

You can use existing external database editing tool such as SQL Server Management Studio to edit initial or testing data, then select the existing database from `DbSessionProvider`, after clicking button `OK`, the static methods will automatically populated with code to initialize the DataSet:

# [C#](#tab/cs)

```cs
private static DataSet<SalesOrderHeader> Headers()
{
    DataSet<SalesOrderHeader> result = DataSet<SalesOrderHeader>.Create().AddRows(4);
    SalesOrderHeader _ = result._;
    _.SuspendIdentity();
    _.SalesOrderID[0] = 1;
    _.SalesOrderID[1] = 2;
    ...
    _.ResumeIdentity();
    return result;
}

private static DataSet<SalesOrderDetail> Details()
{
    DataSet<SalesOrderDetail> result = DataSet<SalesOrderDetail>.Create().AddRows(32);
    SalesOrderDetail _ = result._;
    _.SuspendIdentity();
    _.SalesOrderID[0] = 1;
    _.SalesOrderID[1] = 1;
    ...
    _.ResumeIdentity();
    return result;
}
```

# [VB.Net](#tab/vb)

```vb
Private Shared Function Headers() As DataSet(Of SalesOrderHeader)
    Dim result As DataSet(Of SalesOrderHeader) = DataSet(Of SalesOrderHeader).Create().AddRows(4)
    Dim x As SalesOrderHeader = result.Entity
    x.SuspendIdentity()
    x.SalesOrderID(0) = 1
    x.SalesOrderID(1) = 2
    ...
    x.ResumeIdentity()
    Return result
End Function

Private Shared Function Details() As DataSet(Of SalesOrderDetail)
    Dim result As DataSet(Of SalesOrderDetail) = DataSet(Of SalesOrderDetail).Create().AddRows(32)
    Dim x As SalesOrderDetail = result.Entity
    x.SuspendIdentity()
    x.SalesOrderID(0) = 1
    x.SalesOrderID(1) = 1
    ...
    x.ResumeIdentity()
    Return result
End Function
```

***
