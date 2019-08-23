# Model Keys

Keys are very important part of Relational database model. They are used to establish and identify relationships between tables and also to uniquely identify any record or row of data inside a table.

A Key can be a single column or a group of columns, where the combination may act as a key.

## Primary Key (PK)

A primary key is a special relational database table column (or combination of columns) designated to uniquely identify all table records. Models have primary key should have nested class `PK` and derived from <xref:DevZest.Data.Model`1>:

# [C#](#tab/cs)

```cs
public class Address : BaseModel<Address.PK>
{
    ...
    public sealed class PK : CandidateKey
    {
        public PK(_Int32 addressID)
            : base(addressID)
        {
        }
    }

    protected sealed override PK CreatePrimaryKey()
    {
        return new PK(AddressID);
    }

    [Identity]
    public _Int32 AddressID { get; private set; }
    ...
}
```

# [VB.Net](#tab/vb)

```vb
Public Class Address
    Inherits BaseModel(Of Address.PK)

    Public NotInheritable Class PK
        Inherits CandidateKey

        Public Sub New(addressID As _Int32)
            MyBase.New(addressID)
        End Sub
    End Class

    Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
        Return New PK(AddressID)
    End Function

    Private m_AddressID As _Int32
    <Identity>
    Public Property AddressID As _Int32
        Get
            Return m_AddressID
        End Get
        Private Set
            m_AddressID = Value
        End Set
    End Property
End Class
```

***

## Key/Ref

Optionally, you can define nested class `Key`/`Ref`, which contains exactly the primary key column(s):

# [C#](#tab/cs)

```cs
public class Address : ...
{
    ...
    public class Key : Key<PK>
    {
        static Key()
        {
            Register((Key _) => _.AddressID, _AddressID);
        }

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(AddressID);
        }

        public _Int32 AddressID { get; private set; }
    }

    public class Ref : Ref<PK>
    {
        static Ref()
        {
            Register((Ref _) => _.AddressID, _AddressID);
        }

        public _Int32 AddressID { get; private set; }

        protected override PK CreateForeignKey()
        {
            return new PK(AddressID);
        }
    }
    ...
}
```

# [VB.Net](#tab/vb)

```vb
Public Class Address
    ...
    Public Class Key
        Inherits Key(Of PK)

        Shared Sub New()
            Register(Function(x As Key) x.AddressID, _AddressID)
        End Sub

        Protected Overrides Function CreatePrimaryKey() As PK
            Return New PK(AddressID)
        End Function

        Private m_AddressID As _Int32
        Public Property AddressID As _Int32
            Get
                Return m_AddressID
            End Get
            Private Set
                m_AddressID = Value
            End Set
        End Property
    End Class

    Public Class Ref
        Inherits Ref(Of PK)

        Shared Sub New()
            Register(Function(x As Ref) x.AddressID, _AddressID)
        End Sub

        Private m_AddressID As _Int32
        Public Property AddressID As _Int32
            Get
                Return m_AddressID
            End Get
            Private Set
                m_AddressID = Value
            End Set
        End Property

        Protected Overrides Function CreateForeignKey() As PK
            Return New PK(AddressID)
        End Function
    End Class
    ...
End Class
```

***

The following table shows the difference between nested class `Key` and `Ref`:

| Key | Ref |
|-----|-----|
| Data rows must be unique. Can be used to delete rows from main table. For example, you can put ids into `DataSet<Address.Key>`, to delete these rows from `Address` table, in one server round trip. | Duplicate data rows allowed. Can be used to lookup values from the main table. For example, you can lookup `Address` attribute such as `City` and `StateProvince` for existing ids in `DataSet<Address.Ref>`, in one server round trip. |

## Add PK/Key/Ref

You can add nested class `PK`/`Key`/`Ref` via Model Visualizer, saving you a lot of key strokes and API remembering. Just click the left top drop down button of Model Visualizer tool window, then click 'Add Primary Key...':

![image](/images/model_visualizer_add_pk.jpg)

The following dialog will be displayed:

![image](/images/model_visualizer_add_pk_dialog.jpg)

Click button 'OK', the code will be generated automatically!

Class `Key`/`Ref` can be added together with `PK`, or added later separately.
