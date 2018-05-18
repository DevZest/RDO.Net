Imports DevZest.Data
Imports DevZest.Data.SqlServer
Imports DevZest.Data.Annotations.Primitives

Namespace DevZest.Samples.AdventureWorksLT
    Public NotInheritable Class UdtOrderNumber
        Inherits ColumnAttribute

        Protected Overrides Sub Initialize(ByVal column As Column)
            column.Nullable(True)
            CType(column, Column(Of String)).AsNVarChar(25)
        End Sub
    End Class
End Namespace
