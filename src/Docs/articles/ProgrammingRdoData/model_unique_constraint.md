# Model Unique Constraint

The UNIQUE constraint uniquely identifies each record in a database table or dataset. The UNIQUE and PRIMARY KEY constraints both provide a guarantee for uniqueness for a column or set of columns. A PRIMARY KEY constraint automatically has a UNIQUE constraint defined on it. Note that you can have many UNIQUE constraints per model, but only one PRIMARY KEY constraint per model.

The constraint must be a readonly property which returns a <xref:DevZest.Data.ColumnSort> array, specified by a pair of <xref:DevZest.Data.Annotations.UniqueConstraintAttribute> (at model class level) and <xref:DevZest.Data.Annotations._UniqueConstraintAttribute> (at property level):

# [C#](#tab/cs)

```cs
[UniqueConstraint(nameof(AK_Product_Name), Description = "Unique nonclustered constraint.")]
public class Product : ...
{
    [_UniqueConstraint]
    private ColumnSort[] AK_Product_Name => new ColumnSort[] { Name };
}
```

# [VB.Net](#tab/vb)

```vb
<UniqueConstraint("AK_Product_Name", Description:="Unique nonclustered constraint.")>
Public Class Product
    ...
    <_UniqueConstraint>
    Private ReadOnly Property AK_Product_Name As ColumnSort()
        Get
            Return New ColumnSort() {Name}
        End Get
    End Property
    ...
End Class
```

***

You can add unique constraint by clicking the left top drop down button of Model Visualizer tool window, then click 'Add Unique Constraint...':

![image](/images/model_visualizer_add_ak.jpg)

The following dialog will be displayed:

![image](/images/model_visualizer_add_ak_dialog.jpg)

Fill the dialog form and click "OK", code of an unique constraint will be generated automatically.
