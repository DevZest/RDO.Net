Imports DevZest.Data
Imports DevZest.Data.Presenters
Imports DevZest.Data.Views

Partial Class SalesOrderWindow
    Private Class DetailPresenter
        Inherits DataPresenter(Of SalesOrderInfoDetail)
        Implements ForeignKeyBox.ILookupService
        Implements DataView.IPasteAppendService
        Public Sub New(ownerWindow As Window)
            _ownerWindow = ownerWindow
        End Sub

        Private ReadOnly _ownerWindow As Window

        Protected Overrides Sub BuildTemplate(builder As TemplateBuilder)
            Dim e = Entity
            Dim product = e.Product
            builder.GridRows("Auto", "20") _
                .GridColumns("20", "*", "*", "Auto", "Auto", "Auto", "Auto") _
                .WithFrozenTop(1) _
                .GridLineX(New GridPoint(0, 2), 7) _
                .GridLineY(New GridPoint(2, 1), 1) _
                .GridLineY(New GridPoint(3, 1), 1) _
                .GridLineY(New GridPoint(4, 1), 1) _
                .GridLineY(New GridPoint(5, 1), 1) _
                .GridLineY(New GridPoint(6, 1), 1) _
                .GridLineY(New GridPoint(7, 1), 1) _
                .Layout(Orientation.Vertical) _
                .WithVirtualRowPlacement(VirtualRowPlacement.Tail) _
                .AllowDelete() _
                .AddBinding(0, 0, Me.BindToGridHeader()) _
                .AddBinding(1, 0, product.ProductNumber.BindToColumnHeader("Product No.")) _
                .AddBinding(2, 0, product.Name.BindToColumnHeader("Product")) _
                .AddBinding(3, 0, e.UnitPrice.BindToColumnHeader("Unit Price")) _
                .AddBinding(4, 0, e.UnitPriceDiscount.BindToColumnHeader("Discount")) _
                .AddBinding(5, 0, e.OrderQty.BindToColumnHeader("Qty")) _
                .AddBinding(6, 0, e.LineTotal.BindToColumnHeader("Total")) _
                .AddBinding(0, 1, e.BindTo(Of RowHeader)()) _
                .AddBinding(1, 1, e.FK_Product.BindToForeignKeyBox(product, AddressOf GetProductNumber).MergeIntoGridCell(product.ProductNumber.BindToTextBlock()).WithSerializableColumns(e.ProductID, product.ProductNumber)) _
                .AddBinding(2, 1, product.Name.BindToTextBlock().AddToGridCell().WithSerializableColumns(product.Name)) _
                .AddBinding(3, 1, e.UnitPrice.BindToTextBox().MergeIntoGridCell()) _
                .AddBinding(4, 1, e.UnitPriceDiscount.BindToTextBox(New PercentageConverter()).MergeIntoGridCell(e.UnitPriceDiscount.BindToTextBlock("{0:P}"))) _
                .AddBinding(5, 1, e.OrderQty.BindToTextBox().MergeIntoGridCell()) _
                .AddBinding(6, 1, e.LineTotal.BindToTextBlock("{0:C}").AddToGridCell().WithSerializableColumns(e.LineTotal))
        End Sub

        Private Shared Function GetProductNumber(valueBag As ColumnValueBag, productKey As Product.PK, productLookup As Product.Lookup) As String
            Return valueBag.GetValue(productLookup.ProductNumber)
        End Function

        Private Function CanLookup(foreignKey As CandidateKey) As Boolean Implements ForeignKeyBox.ILookupService.CanLookup
            If foreignKey Is Entity.FK_Product Then
                Return True
            Else
                Return False
            End If
        End Function

        Private Sub BeginLookup(foreignKeyBox As ForeignKeyBox) Implements ForeignKeyBox.ILookupService.BeginLookup
            If foreignKeyBox.ForeignKey Is Entity.FK_Product Then
                Dim dialogWindow = New ProductLookupWindow()
                dialogWindow.Show(_ownerWindow, foreignKeyBox, CurrentRow.GetValue(Entity.ProductID))
            Else
                Throw New NotSupportedException()
            End If
        End Sub

        Protected Overrides Function ConfirmDelete() As Boolean
            Return MessageBox.Show(String.Format("Are you sure you want to delete selected {0} rows?", SelectedRows.Count), "Delete", MessageBoxButton.YesNo) = MessageBoxResult.Yes
        End Function

        Private Function Verify(data As IReadOnlyList(Of ColumnValueBag)) As Boolean Implements DataView.IPasteAppendService.Verify
            Dim foreignKeys = DevZest.Data.DataSet(Of Product.Ref).Create()
            For i = 0 To data.Count
                Dim valueBag = data(i)
                Dim productId = If(valueBag.ContainsKey(Entity.ProductID), valueBag(Entity.ProductID), Nothing)
                foreignKeys.AddRow(Sub(e, dataRow) e.ProductID.SetValue(dataRow, productId))
            Next

            Dim lookup As DataSet(Of Product.Lookup) = Nothing
            If Not App.Execute(Function(db, ct) db.LookupAsync(foreignKeys, ct), Window.GetWindow(View), lookup) Then
                Return False
            End If

            Debug.Assert(lookup.Count = data.Count)
            Dim product = Entity.Product
            For i = 0 To lookup.Count
                data(i).SetValue(product.Name, lookup.Entity.Name(i))
                data(i).SetValue(product.ProductNumber, lookup.Entity.ProductNumber(i))
            Next
            Return True
        End Function
    End Class
End Class
