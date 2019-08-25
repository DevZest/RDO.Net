# Model Check Constraint

A check constraint is a type of integrity constraint which specifies a requirement that must be met by each row in a database table or dataset. It can refer to a single column, or multiple columns of the model.

The constraint must be a predicate of <xref:DevZest.Data._Boolean> expression column, specified by a pair of <xref:DevZest.Data.Annotations.CheckConstraintAttribute> (at model class level) and <xref:DevZest.Data.Annotations._CheckConstraintAttribute> (at property level):

# [C#](#tab/cs)

```cs
[CheckConstraint(nameof(CK_SalesOrderDetail_OrderQty), typeof(UserMessages), nameof(UserMessages.CK_SalesOrderDetail_OrderQty), Description = "Check constraint [OrderQty] > (0)")]
public class SalesOrderDetail : ...
{
    ...
    [_CheckConstraint]
    private _Boolean CK_SalesOrderDetail_OrderQty
    {
        get { return OrderQty > _Decimal.Const(0); }
    }
    ...
}
```

# [VB.Net](#tab/vb)

```vb
<CheckConstraint("CK_SalesOrderDetail_OrderQty", GetType(My.UserMessages), NameOf(My.UserMessages.CK_SalesOrderDetail_OrderQty), Description:="Check constraint [OrderQty] > (0)")>
Public Class SalesOrderDetail
    ...
    <_CheckConstraint>
    Private ReadOnly Property CK_SalesOrderDetail_OrderQty As _Boolean
        Get
            Return OrderQty > _Decimal.Const(0)
        End Get
    End Property
    ...
End Class
```

***

You can add check constraint by clicking the left top drop down button of Model Visualizer tool window, then click 'Add Check Constraint...':

![image](/images/model_visualizer_add_ck.jpg)

The following dialog will be displayed:

![image](/images/model_visualizer_add_ck_dialog.jpg)

Fill the dialog form and click "OK", code of a to-be-implemented check constraint will be generated automatically.
