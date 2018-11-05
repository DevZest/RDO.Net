Imports DevZest.Data
Imports DevZest.Data.SqlServer
Imports DevZest.Data.Annotations.Primitives
Imports DevZest.Data.Addons

Namespace DevZest.Samples.AdventureWorksLT
    <ModelMemberAttributeSpec(New Type() {GetType(ColumnNotNull), GetType(SqlColumnDescriptor)}, True, GetType(_String))>
    Public NotInheritable Class UdtAccountNumberAttribute
        Inherits ColumnAttribute

        Protected Overrides Sub Wireup(column As Column)
            If TypeOf column Is _String Then
                column.Nullable(True)
                CType(column, _String).AsSqlNVarChar(15)
            End If
        End Sub
    End Class
End Namespace
