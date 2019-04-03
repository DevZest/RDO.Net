Imports DevZest.Data.Annotations.Primitives
Imports DevZest.Data.Addons

<ModelDesignerSpec(New Type() {GetType(ColumnNotNull)}, New Type() {GetType(_Boolean)})>
Public NotInheritable Class UdtFlagAttribute
    Inherits ColumnAttribute

    Protected Overrides Sub Wireup(column As Column)
        If TypeOf column Is _Boolean Then
            column.Nullable(False)
        End If
    End Sub
End Class
