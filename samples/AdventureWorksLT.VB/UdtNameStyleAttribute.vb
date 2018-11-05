Imports DevZest.Data
Imports DevZest.Data.Annotations.Primitives
Imports DevZest.Data.Addons

Namespace DevZest.Samples.AdventureWorksLT
    <ModelMemberAttributeSpec(GetType(ColumnNotNull), True, GetType(_Boolean))>
    Public NotInheritable Class UdtNameStyleAttribute
        Inherits ColumnAttribute

        Protected Overrides Sub Wireup(column As Column)
            If TypeOf column Is _Boolean Then
                column.Nullable(False)
            End If
        End Sub
    End Class
End Namespace
