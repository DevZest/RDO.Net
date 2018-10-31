Imports DevZest.Data
Imports DevZest.Data.Annotations.Primitives

Namespace DevZest.Samples.AdventureWorksLT
    Public NotInheritable Class UdtFlagAttribute
        Inherits ColumnAttribute

        Protected Overrides Sub Wireup(column As Column)
            column.Nullable(False)
        End Sub
    End Class
End Namespace
