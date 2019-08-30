Imports System.Threading
Imports DevZest.Data
Imports DevZest.Data.Primitives

#If DbInit Then
Imports DevZest.Data.DbInit
#End If

Public Class MockProductCategory
    Inherits DbMock(Of Db)

    Public Shared Function CreateAsync(db As Db, Optional progress As IProgress(Of DbInitProgress) = Nothing, Optional ct As CancellationToken = Nothing) As Task(Of Db)
        Return New MockProductCategory().MockAsync(db, progress, ct)
    End Function

    Private Shared Function MockData() As DataSet(Of ProductCategory)
        Dim result As DataSet(Of ProductCategory) = DataSet(Of ProductCategory).Create().AddRows(13)
        Dim x As ProductCategory = result.Entity
        x.SuspendIdentity()
        x.ProductCategoryID(0) = 1
        x.ProductCategoryID(1) = 2
        x.ProductCategoryID(2) = 3
        x.ProductCategoryID(3) = 4
        x.ProductCategoryID(4) = 5
        x.ProductCategoryID(5) = 6
        x.ProductCategoryID(6) = 7
        x.ProductCategoryID(7) = 8
        x.ProductCategoryID(8) = 9
        x.ProductCategoryID(9) = 10
        x.ProductCategoryID(10) = 11
        x.ProductCategoryID(11) = 12
        x.ProductCategoryID(12) = 13
        x.ParentProductCategoryID(0) = Nothing
        x.ParentProductCategoryID(1) = Nothing
        x.ParentProductCategoryID(2) = 1
        x.ParentProductCategoryID(3) = 1
        x.ParentProductCategoryID(4) = 1
        x.ParentProductCategoryID(5) = 1
        x.ParentProductCategoryID(6) = 2
        x.ParentProductCategoryID(7) = 6
        x.ParentProductCategoryID(8) = 6
        x.ParentProductCategoryID(9) = 6
        x.ParentProductCategoryID(10) = 7
        x.ParentProductCategoryID(11) = 7
        x.ParentProductCategoryID(12) = 7
        x.Name(0) = "Bikes"
        x.Name(1) = "Other"
        x.Name(2) = "Mountain Bikes"
        x.Name(3) = "Road Bikes"
        x.Name(4) = "Touring Bikes"
        x.Name(5) = "Components"
        x.Name(6) = "Clothing"
        x.Name(7) = "Handlebars"
        x.Name(8) = "Bottom Brackets"
        x.Name(9) = "Brakes"
        x.Name(10) = "Bib-Shorts"
        x.Name(11) = "Caps"
        x.Name(12) = "Gloves"
        x.RowGuid(0) = New Guid("cfbda25c-df71-47a7-b81b-64ee161aa37c")
        x.RowGuid(1) = New Guid("09e91437-ba4f-4b1a-8215-74184fd95db8")
        x.RowGuid(2) = New Guid("2d364ade-264a-433c-b092-4fcbf3804e01")
        x.RowGuid(3) = New Guid("000310c0-bcc8-42c4-b0c3-45ae611af06b")
        x.RowGuid(4) = New Guid("02c5061d-ecdc-4274-b5f1-e91d76bc3f37")
        x.RowGuid(5) = New Guid("c657828d-d808-4aba-91a3-af2ce02300e9")
        x.RowGuid(6) = New Guid("10a7c342-ca82-48d4-8a38-46a2eb089b74")
        x.RowGuid(7) = New Guid("3ef2c725-7135-4c85-9ae6-ae9a3bdd9283")
        x.RowGuid(8) = New Guid("a9e54089-8a1e-4cf5-8646-e3801f685934")
        x.RowGuid(9) = New Guid("d43ba4a3-ef0d-426b-90eb-4be4547dd30c")
        x.RowGuid(10) = New Guid("67b58d2b-5798-4a90-8c6c-5ddacf057171")
        x.RowGuid(11) = New Guid("430dd6a8-a755-4b23-bb05-52520107da5f")
        x.RowGuid(12) = New Guid("92d5657b-0032-4e49-bad5-41a441a70942")
        x.ModifiedDate(0) = Convert.ToDateTime("2002-06-01T00:00:00.000")
        x.ModifiedDate(1) = Convert.ToDateTime("2002-06-01T00:00:00.000")
        x.ModifiedDate(2) = Convert.ToDateTime("2002-06-01T00:00:00.000")
        x.ModifiedDate(3) = Convert.ToDateTime("2002-06-01T00:00:00.000")
        x.ModifiedDate(4) = Convert.ToDateTime("2002-06-01T00:00:00.000")
        x.ModifiedDate(5) = Convert.ToDateTime("2002-06-01T00:00:00.000")
        x.ModifiedDate(6) = Convert.ToDateTime("2002-06-01T00:00:00.000")
        x.ModifiedDate(7) = Convert.ToDateTime("2002-06-01T00:00:00.000")
        x.ModifiedDate(8) = Convert.ToDateTime("2002-06-01T00:00:00.000")
        x.ModifiedDate(9) = Convert.ToDateTime("2002-06-01T00:00:00.000")
        x.ModifiedDate(10) = Convert.ToDateTime("2002-06-01T00:00:00.000")
        x.ModifiedDate(11) = Convert.ToDateTime("2002-06-01T00:00:00.000")
        x.ModifiedDate(12) = Convert.ToDateTime("2002-06-01T00:00:00.000")
        x.ResumeIdentity()
        Return result
    End Function

    Protected Overrides Sub Initialize()
        Mock(Db.ProductCategory, AddressOf MockData)
    End Sub
End Class
