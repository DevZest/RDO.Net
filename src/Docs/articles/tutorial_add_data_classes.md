---
uid: tutorial_add_data_classes
---

# Add Data Classes

In this section, a `Movie` model class and a `Db` database session class will be added into *Movies* project.

## Get Started

Before continue, make the following changes to *Movies* project:

* Delete *Class1.cs* (or *Class1.vb* if you're using VB.Net) from the project.
* Add NuGet Package [DevZest.Data.SqlServer](https://www.nuget.org/packages/DevZest.Data.SqlServer/) to this project.

## Add Movie Class

### Step 1. Add new class

Add new class `Movie` into project *Movies*, inherits from <xref:DevZest.Data.Model>:

# [C#](#tab/cs)

```csharp
using DevZest.Data;

namespace Movies
{
    public class Movie : Model
    {
    }
}
```

# [VB.Net](#tab/vb)

```vb
Imports DevZest.Data

Public Class Movie
    Inherits Model

End Class
```

***

The class can be viewed in *Model Visualizer* tool window:

![image](/images/tutorial_model_visualizer_empty_movie.jpg)

>[!Note]
>You can show *Model Visualizer* tool window by clicking menu "*View*" -> "*Other Windows*" -> "*Model Visualizer*" in Visual Studio.

### Step 2. Add ID field

In *Model Visualizer* tool window, click the left top ![image](/images/model_visualizer_add.jpg) button, the following code snippet will be inserted:

# [C#](#tab/cs)

![image](/images/model_visualizer_add_property_cs.jpg)

# [VB.Net](#tab/vb)

![image](/images/model_visualizer_add_property_vb.jpg)

***

Tabbing through the code snippet to change the property name to `ID` and property type to `_Int32`. When done, press ESC to quit code snippet editing. You now have a readonly property `ID` as type `_Int32`, with a compile-time warning `Missing registration for property 'ID'`. You can fix this warning by moving the caret to the property name, and pressing *CTRL-.* in Visual Studio:

# [C#](#tab/cs)

![image](/images/tutorial_add_mounter_cs.jpg)

# [VB.Net](#tab/vb)

![image](/images/tutorial_add_mounter_vb.jpg)

***

Select *Add Mounter _ID* in the dropdown menu, RDO.Tools will automatically insert following code to register the property:

# [C#](#tab/cs)

```csharp
...
public static readonly Mounter<_Int32> _ID = RegisterColumn((Movie _) => _.ID);
...
```

# [VB.Net](#tab/vb)

```vb
...
Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As Movie) x.ID)
...
```

***

***

### Step 3. Annotate ID field

In *Model Visualizer* tool window, right click *ID* field, available annotations for current field will be displayed as context menu:

![image](/images/tutorial_add_identity.jpg)

Click *Identity*, field `ID` will be annotated with `Identity` attribute:

# [C#](#tab/cs)

```csharp
using DevZest.Data;
using DevZest.Data.Annotations;

namespace Movies
{
    public class Movie : Model
    {
        public static readonly Mounter<_Int32> _ID = RegisterColumn((Movie _) => _.ID);

        [Identity]
        public _Int32 ID { get; private set; }
    }
}
```

# [VB.Net](#tab/vb)

```vb
Imports DevZest.Data
Imports DevZest.Data.Annotations

Public Class Movie
    Inherits Model

    Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As Movie) x.ID)

    Private m_ID As _Int32
    <Identity>
    Public Property ID As _Int32
        Get
            Return m_ID
        End Get
        Private Set
            m_ID = Value
        End Set
    End Property
End Class
```

***

### Step 4. Add primary key

In *Model Visualizer* tool window, click the left top ![image](/images/model_visualizer_add.jpg) dropdown button, then click *Add Primary Key...* from the drop down menu:

![image](/images/tutorial_add_primary_key_menu.jpg)

The following dialog will be displayed:

![image](/images/tutorial_add_primary_key.jpg)

Field `ID` is detected automatically. Click *OK* button in the dialog, the following code will be generated automatically:

# [C#](#tab/cs)

```csharp
...
public class Movie : Model<Movie.PK>
{
    public sealed class PK : CandidateKey
    {
        public PK(_Int32 id) : base(id)
        {
        }
    }

    protected sealed override PK CreatePrimaryKey()
    {
        return new PK(ID);
    }

    public class Key : Key<PK>
    {
        static Key()
        {
            Register((Key _) => _.ID, _ID);
        }

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(ID);
        }

        public _Int32 ID { get; private set; }
    }
...
}
```

# [VB.Net](#tab/vb)

```vb
Public Class Movie
    Inherits Model(Of Movie.PK)

    Public NotInheritable Class PK
        Inherits CandidateKey

        Public Sub New(id As _Int32)
            MyBase.New(id)
        End Sub
    End Class

    Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
        Return New PK(ID)
    End Function

    Public Class Key
        Inherits Key(Of PK)

        Shared Sub New()
            Register(Function(x As Key) x.ID, _ID)
        End Sub

        Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
            Return New PK(ID)
        End Function

        Private m_ID As _Int32

        Public Property ID As _Int32
            Get
                Return m_ID
            End Get
            Private Set
                m_ID = Value
            End Set
        End Property
    End Class
    ...
End Class
```

***

### Step 5. Add other fields

Add following fields using steps described previously:

# [C#](#tab/cs)

| Name | Type | Annotation(s) |
|------|------|---------------|
| Title | _String | `[StringLength(60, MinimumLength = 3)]`, `[Required]`, `[SqlNVarChar(60)]` |
| ReleaseDate | _DateTime | `Display(Name = "Release Date")]`, `[SqlDate]`, `[Required]` |
| Genre | _String | `[RegularExpression(@"^[A-Z]+[a-zA-Z""'\s-]*$")]`, `[Required]`, `[StringLength(30)]`, `[SqlNVarChar(30)]` |
| Price | _Decimal | `[SqlMoney]`, `[Required]` |

# [VB.Net](#tab/vb)

| Name | Type | Annotation(s) |
|------|------|---------------|
| Title | _String | `<StringLength(60, MinimumLength:=3)>`, `<Required>`, `<SqlNVarChar(60)>` |
| ReleaseDate | _DateTime | `<Display(Name:="Release Date")>`, `<SqlDate>`, `<Required>` |
| Genre | _String | `<RegularExpression("^[A-Z]+[a-zA-Z""'\s-]*$")>`, `<Required>`, `<StringLength(30)>`, `<SqlNVarChar(30)>` |
| Price | _Decimal | `<SqlMoney>`, `<Required>` |

***

***

The final `Movie` class:

# [C#](#tab/cs)

[!code-csharp[Db](../../../samples/Tutorial/Movies/Movie.cs)]

# [VB.Net](#tab/vb)

[!code-csharp[Db](../../../samples.vb/Tutorial/Movies/Movie.vb)]

***

## Add Db Class

### Step 1. Add new class

Add new class `Db` into project *Movies*, inherits from <xref:DevZest.Data.SqlServer.SqlSession>:

# [C#](#tab/cs)

```csharp
using DevZest.Data.SqlServer;
using System.Data.SqlClient;

namespace Movies
{
    public partial class Db : SqlSession
    {
        public Db(string connectionString)
            : this(new SqlConnection(connectionString))
        {
        }

        public Db(SqlConnection sqlConnection)
            : base(sqlConnection)
        {
        }
    }
}
```

# [VB.Net](#tab/vb)

```vb
Imports System.Data.SqlClient
Imports DevZest.Data.SqlServer

Partial Public Class Db
    Inherits SqlSession
    Public Sub New(connectionString As String)
        Me.New(New SqlConnection(connectionString))
    End Sub

    Public Sub New(sqlConnection As SqlConnection)
        MyBase.New(sqlConnection)
    End Sub
End Class
```

***

The class can be viewed in *Db Visualizer* tool window:

![image](/images/tutorial_db_visualizer_empty_db.jpg)

>[!Note]
>You can show *Db Visualizer* tool window by clicking menu "*View*" -> "*Other Windows*" -> "*Db Visualizer*" in Visual Studio.

### Add table property

In *Db Visualizer* tool window, click the left top ![image](/images/db_visualizer_add_table.jpg) button, the following dialog will be displayed:

![image](/images/tutorial_add_table.jpg)

Select *Movie* from *Model:* combo box, then click button *OK*, a `Movie` property will be generated in class `Db`:

# [C#](#tab/cs)

[!code-csharp[Db](../../../samples/Tutorial/Movies/Db.cs)]

# [VB.Net](#tab/vb)

[!code-csharp[Db](../../../samples.vb/Tutorial/Movies/Db.vb)]

***
