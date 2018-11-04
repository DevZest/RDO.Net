Imports DevZest.Data
Imports DevZest.Data.Primitives
Imports DevZest.Data.SqlServer
Imports DevZest.Data.Annotations.Primitives

Namespace DevZest.Samples.AdventureWorksLT
    <ModelMemberAttributeSpec(New Type() {GetType(ColumnNotNull), GetType(SqlColumnDescriptor)}, True, GetType(_String))>
    Public NotInheritable Class UdtNameAttribute
        Inherits ColumnAttribute

        Protected Overrides Sub Wireup(ByVal column As Column)
            If TypeOf column Is _String Then
                column.Nullable(True)
                CType(column, _String).AsSqlNVarChar(50)
            End If
        End Sub
    End Class
End Namespace
