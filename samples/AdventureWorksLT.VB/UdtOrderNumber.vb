Imports DevZest.Data
Imports DevZest.Data.Primitives
Imports DevZest.Data.SqlServer
Imports DevZest.Data.Annotations.Primitives

Namespace DevZest.Samples.AdventureWorksLT
    <ModelMemberAttributeSpec(New Type() {GetType(NotNull), GetType(SqlColumnDescriptor)}, True, GetType(_String))>
    Public NotInheritable Class UdtOrderNumber
        Inherits ColumnAttribute

        Protected Overrides Sub Wireup(ByVal column As Column)
            If TypeOf column Is _String Then
                column.Nullable(True)
                CType(column, _String).AsSqlNVarChar(25)
            End If
        End Sub
    End Class
End Namespace
