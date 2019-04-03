Imports DevZest.Data.Annotations.Primitives
Imports DevZest.Data.Addons

<ModelDesignerSpec(New Type() {GetType(ColumnNotNull), GetType(SqlType)}, New Type() {GetType(_String)})>
Public NotInheritable Class UdtNameAttribute
    Inherits ColumnAttribute

    Protected Overrides Sub Wireup(ByVal column As Column)
        If TypeOf column Is _String Then
            column.Nullable(True)
            CType(column, _String).AsSqlNVarChar(50)
        End If
    End Sub
End Class
