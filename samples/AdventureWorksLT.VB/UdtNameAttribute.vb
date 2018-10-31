Imports DevZest.Data
Imports DevZest.Data.SqlServer
Imports DevZest.Data.Annotations.Primitives

Namespace DevZest.Samples.AdventureWorksLT
    Public NotInheritable Class UdtNameAttribute
        Inherits ColumnAttribute

        Protected Overrides Sub Wireup(ByVal column As Column)
            column.Nullable(True)
            CType(column, Column(Of String)).AsNVarChar(50)
        End Sub
    End Class
End Namespace
