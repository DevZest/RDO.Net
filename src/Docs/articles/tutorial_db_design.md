---
uid: tutorial_db_design
---

# Design Time Database

In this section, we will work on *Movies.DbDesign* project to manipulate design time database.

## Get Started

Before continue, make the following changes to *Movies.DbDesign* project:

*Step 1*. Add project reference `Movies` to this project.

*Step 2*. Add NuGet Package [DevZest.Data.DbDesign](https://www.nuget.org/packages/DevZest.Data.DbDesign/) to this project.

*Step 3*. Change default namespace (root namespace in Visual Basic) from `Movies.DbDesign` to `Movies`, by right clicking the project `Movies.DbDesign` in *Solution Explorer* tool window, then click *Properties* in context menu:

# [C#](#tab/cs)

![image](/images/tutorial_movies_dbdesign_default_namespace.jpg)

# [VB.Net](#tab/vb)

![image](/images/tutorial_movies_dbdesign_root_namespace.jpg)

***

***

*Step 4*. Change *Program.cs* (*Program.vb* in Visual Basic) to following:

# [C#](#tab/cs)

[!code-csharp[Program](../../../samples/Tutorial/Movies.DbDesign/Program.cs)]

# [VB.Net](#tab/vb)

[!code-csharp[Program](../../../samples.vb/Tutorial/Movies.DbDesign/Program.vb)]

***

## Create LocalDB

*Step 1*. Create a *LocalDb* subfolder by right clicking *Movies.DbDesign* project in *Solution Explorer* tool window, then click "*Add*" -> "*New Folder*" in the context menu.

*Step 2*. Open *SQL Server Management Studio*, use Server name *(localdb)\MSSQLLocalDB*, and select *Windows Authentication*:

![image](/images/ssms_login.jpg)

*Step 3*. Create a new query in *SQL Server Management Studio* by pressing CTRL-N, then execute the following query:

```SQL
CREATE DATABASE EmptyDb ON (
  NAME='EmptyDb', 
  FILENAME='%path_to_your_solution%\Movies.DbDesign\LocalDb\EmptyDb.mdf')
```

You should have 2 files *EmptyDb.mdf* and *EmptyDb_log.ldf* created under the folder created in *Step 1*.

*Step 4*. Add class *_DevDb* to project *Movies.DbDesign*:

# [C#](#tab/cs)

[!code-csharp[_DevDb](../../../samples/Tutorial/Movies.DbDesign/_DevDb.cs)]

# [VB.Net](#tab/vb)

[!code-csharp[_DevDb](../../../samples.vb/Tutorial/Movies.DbDesign/_DevDb.vb)]

***

***

*Step 5*. Right click in code editor of class *_DevDb*, click "*Generate Db...*" context menu item:

![image](/images/RdoToolsGenerateDb.jpg)

Click button *OK*, a database named *Movies* containing a table *Movie* will be generated. You can verify it under folder created in *Step1* and *SQL Server Management Studio*.

>[!Note]
>Database files *Movie.mdf* and *Movie_log.ldf* should be ignored from source control.

## Add Testing Data

*Step 1*. In *SQL Server Management Studio*, right click table *dbo.Movie* of the database *Movies* created previously, then click *Edit Top 200 Rows* context menu item to add following testing data:

![image](/images/tutorial_movies_testing_data.jpg)

*Step 2*. Add class *DevDb* (without leading underscore) to project *Movies.DbDesign*:

# [C#](#tab/cs)

[!code-csharp[_DevDb](../../../samples/Tutorial/Movies.DbDesign/DevDb.cs)]

# [VB.Net](#tab/vb)

[!code-vb[_DevDb](../../../samples.vb/Tutorial/Movies.DbDesign/DevDb.vb)]

***

***

*Step 3*. Add class *MockMovie* to project *Movies.DbDesign*:

# [C#](#tab/cs)

```csharp
using DevZest.Data;

namespace Movies
{
    public sealed class MockMovie : DbMock<Db>
    {
        private static DataSet<Movie> GetMovies()
        {
            return DataSet<Movie>.Create();
        }

        protected override void Initialize()
        {
            Mock(Db.Movie, GetMovies);
        }
    }
}
```

# [VB.Net](#tab/vb)

```vb
Imports DevZest.Data

Public Class MockMovie
    Inherits DbMock(Of Db)

    Private Shared Function GetMovies() As DataSet(Of Movie)
        Throw New System.NotImplementedException()
    End Function

    Protected Overrides Sub Initialize()
        Mock(Db.Movie, AddressOf GetMovies)
    End Sub
End Class
```

***

There is a compile-time warning: *DbMock implementation should expose a static factory method to wrap a call to DbMock<T>.MockAsync method*. Fix this warning by moving the caret to the class name *MockMovie*, then pressing *CTRL-.* in Visual Studio. Select context menu item *Add factory method*, the following code will be inserted automatically:

# [C#](#tab/cs)

```csharp
public static Task<Db> CreateAsync(Db db, IProgress<DbInitProgress> progress = null, CancellationToken ct = default(CancellationToken))
{
    return new MockMovie().MockAsync(db, progress, ct);
}
```

# [VB.Net](#tab/vb)

```vb
Public Shared Function CreateAsync(db As Db, Optional progress As IProgress(Of DbInitProgress) = Nothing, Optional ct As CancellationToken = Nothing) As Task(Of Db)
    Return New MockMovie().MockAsync(db, progress, ct)
End Function
```

***

***

*Step 4*. Right click in code editor of class *MockMovie*, click "*Generate DataSet(s)...*" context menu item:

![image](/images/tutorial_movies_generate_dataset.jpg)

Click button *OK*, the code for data set will be generated automatically:

# [C#](#tab/cs)

[!code-csharp[MockMovie](../../../samples/Tutorial/Movies.DbDesign/MockMovie.cs)]

# [VB.Net](#tab/vb)

[!code-vb[MockMovie](../../../samples.vb/Tutorial/Movies.DbDesign/MockMovie.vb)]

***
