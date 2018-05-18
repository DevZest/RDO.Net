Imports System
Imports DevZest.Data
Imports DevZest.Data.SqlServer
Imports DevZest.Data.Annotations.Primitives

Namespace DevZest.Samples.AdventureWorksLT
    Public NotInheritable Class UdtAccountNumberAttribute
        Inherits ColumnAttribute

        Protected Overrides Sub Initialize(column As Column)
            column.Nullable(True)
            CType(column, Column(Of String)).AsNVarChar(15)
        End Sub
    End Class
End Namespace
