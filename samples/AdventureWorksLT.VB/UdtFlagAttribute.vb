Imports DevZest.Data
Imports DevZest.Data.Primitives
Imports DevZest.Data.Annotations.Primitives

Namespace DevZest.Samples.AdventureWorksLT
    <ModelMemberAttributeSpec(GetType(NotNull), True, GetType(_Boolean))>
    Public NotInheritable Class UdtFlagAttribute
        Inherits ColumnAttribute

        Protected Overrides Sub Wireup(column As Column)
            If TypeOf column Is _Boolean Then
                column.Nullable(False)
            End If
        End Sub
    End Class
End Namespace
