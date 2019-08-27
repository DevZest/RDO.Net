---
uid: model_projection
---

# Model Projection

In Relational algebra, projection means collecting a subset of columns for use in operations, i.e. a projection is the list of columns selected.

Typically projection is used to lookup data from one model to the master data model, and it's defined as nested class derived from <xref:DevZest.Data.Projection>:

# [C#](#tab/cs)

```cs
public class Product : ...
{
    ...
    public class Lookup : Projection
    {
        static Lookup()
        {
            Register((Lookup _) => _.Name, _Name);
            Register((Lookup _) => _.ProductNumber, _ProductNumber);
        }

        public _String Name { get; private set; }

        public _String ProductNumber { get; private set; }
    }
    ...
}
```

# [VB.Net](#tab/vb)

```vb
Public Class Product
    ...
    Public Class Lookup
        Inherits Projection

        Shared Sub New()
            Register(Function(x As Lookup) x.Name, _Name)
            Register(Function(x As Lookup) x.ProductNumber, _ProductNumber)
        End Sub

        Private m_Name As _String
        Public Property Name As _String
            Get
                Return m_Name
            End Get
            Private Set
                m_Name = Value
            End Set
        End Property

        Private m_ProductNumber As _String
        Public Property ProductNumber As _String
            Get
                Return m_ProductNumber
            End Get
            Private Set
                m_ProductNumber = Value
            End Set
        End Property
    End Class
    ...
End Class
```

***

The defined projection class can be used for other model:

# [C#](#tab/cs)

```cs
public class SalesOrderInfoDetail : ...
{
    static SalesOrderInfoDetail()
    {
        RegisterProjection((SalesOrderInfoDetail _) => _.Product);
    }

    public Product.Lookup Product { get; private set; }
}
```

# [VB.Net](#tab/vb)

```vb
Public Class SalesOrderInfoDetail
    ...
    Shared Sub New()
        RegisterProjection(Function(x As SalesOrderInfoDetail) x.Product)
    End Sub

    Private m_Product As Product.Lookup
    Public Property Product As Product.Lookup
        Get
            Return m_Product
        End Get
        Private Set
            m_Product = Value
        End Set
    End Property
    ...
End Class
```

***

You can add projection by clicking the left top drop down button of Model Visualizer tool window, then click 'Add Projection...':

![image](/images/model_visualizer_add_projection.jpg)

The following dialog will be displayed:

![image](/images/model_visualizer_add_projection_dialog.jpg)

Fill the dialog form and click "OK", code of a projection will be generated automatically.
