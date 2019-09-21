Imports System.Threading
Imports DevZest.Data
Imports DevZest.Data.Presenters
Imports DevZest.Data.Views

Partial Class MainWindow

    Public NotInheritable Class Styles
        Public Shared ReadOnly CheckBox As New StyleId(GetType(MainWindow))
        Public Shared ReadOnly LeftAlignedTextBlock As New StyleId(GetType(MainWindow))
        Public Shared ReadOnly RightAlignedTextBlock As New StyleId(GetType(MainWindow))
        Public Shared ReadOnly Label As New StyleId(GetType(MainWindow))
    End Class

    Private Class Presenter
        Inherits DataPresenter(Of SalesOrderHeader)
        Implements ColumnHeader.ISortService

        Private ReadOnly _frozenLine As Pen

        Public Sub New()
            _frozenLine = New Pen(Brushes.Black, 1)
            _frozenLine.Freeze()
        End Sub

        Private Function CalcTotalAmt() As Decimal?
            Return Rows.Sum(Function(x) x.GetValue(Entity.TotalDue))
        End Function

        Private ReadOnly Property CalcTotalAmtFunc() As Func(Of Decimal?)
            Get
                Return AddressOf CalcTotalAmt
            End Get
        End Property

        Private _searchText As String
        Public Property SearchText() As String
            Get
                Return _searchText
            End Get
            Set
                _searchText = Value
                RefreshAsync()
            End Set
        End Property

        Private _orderBy As IReadOnlyList(Of IColumnComparer) = New IColumnComparer() {}
        Overloads Property OrderBy() As IReadOnlyList(Of IColumnComparer) Implements ColumnHeader.ISortService.OrderBy
            Get
                Return _orderBy
            End Get
            Set
                _orderBy = Value
                RefreshAsync()
            End Set
        End Property

        Private Function LoadDataAsync(ct As CancellationToken) As Task(Of DataSet(Of SalesOrderHeader))
            Return App.ExecuteAsync(Function(db) db.GetSalesOrderHeadersAsync(SearchText, _orderBy, ct))
        End Function

        Public Overloads Sub ShowAsync(dataView As DataView)
            ShowAsync(dataView, AddressOf LoadDataAsync)
        End Sub

        Public Overloads Function RefreshAsync() As Task
            Return RefreshAsync(AddressOf LoadDataAsync)
        End Function

        Protected Overrides Sub BuildTemplate(builder As TemplateBuilder)
            Dim e = Entity
            builder.GridColumns("20", "50", "70", "70", "75", "60", "100", "70", "70", "75") _
                .GridRows("Auto", "Auto", "Auto") _
                .Layout(Orientation.Vertical) _
                .WithFrozenLeft(2) _
                .WithFrozenRight(1) _
                .WithFrozenTop(1) _
                .WithFrozenBottom(1) _
                .WithStretches(1) _
                .GridLineX(New GridPoint(0, 1), 10) _
                .GridLineX(New GridPoint(0, 2), 10) _
                .GridLineY(New GridPoint(1, 0), 2) _
                .GridLineY(New GridPoint(2, 0), 2, _frozenLine) _
                .GridLineY(New GridPoint(3, 0), 2) _
                .GridLineY(New GridPoint(4, 0), 2) _
                .GridLineY(New GridPoint(5, 0), 2) _
                .GridLineY(New GridPoint(6, 0), 2) _
                .GridLineY(New GridPoint(7, 0), 2) _
                .GridLineY(New GridPoint(8, 0), 2) _
                .GridLineY(New GridPoint(9, 0), 3, _frozenLine, GridPlacement.Head).GridLineY(New GridPoint(10, 0), 3) _
                .AddBinding(0, 0, Me.BindToCheckBox().WithStyle(Styles.CheckBox)) _
                .AddBinding(1, 0, e.SalesOrderID.BindToColumnHeader("ID")) _
                .AddBinding(2, 0, e.SalesOrderNumber.BindToColumnHeader("Number")) _
                .AddBinding(3, 0, e.DueDate.BindToColumnHeader("Due Date")) _
                .AddBinding(4, 0, e.ShipDate.BindToColumnHeader("Ship Date")) _
                .AddBinding(5, 0, e.Status.BindToColumnHeader("Status")) _
                .AddBinding(6, 0, e.PurchaseOrderNumber.BindToColumnHeader("PO Number")) _
                .AddBinding(7, 0, e.SubTotal.BindToColumnHeader("Sub Total")) _
                .AddBinding(8, 0, e.TaxAmt.BindToColumnHeader("Tax Amt")) _
                .AddBinding(9, 0, e.TotalDue.BindToColumnHeader("Total Due")) _
                .AddBinding(0, 1, e.BindToCheckBox().WithStyle(Styles.CheckBox)) _
                .AddBinding(1, 1, e.SalesOrderID.BindToHyperlink(ApplicationCommands.Open).WithStyle(Styles.LeftAlignedTextBlock)) _
                .AddBinding(2, 1, e.SalesOrderNumber.BindToTextBlock().WithStyle(Styles.LeftAlignedTextBlock)) _
                .AddBinding(3, 1, e.DueDate.BindToTextBlock("{0:d}").WithStyle(Styles.RightAlignedTextBlock)) _
                .AddBinding(4, 1, e.ShipDate.BindToTextBlock("{0:d}").WithStyle(Styles.RightAlignedTextBlock)) _
                .AddBinding(5, 1, e.Status.BindToTextBlock().WithStyle(Styles.LeftAlignedTextBlock)) _
                .AddBinding(6, 1, e.PurchaseOrderNumber.BindToTextBlock().WithStyle(Styles.LeftAlignedTextBlock)) _
                .AddBinding(7, 1, e.SubTotal.BindToTextBlock("{0:C}").WithStyle(Styles.RightAlignedTextBlock)) _
                .AddBinding(8, 1, e.TaxAmt.BindToTextBlock("{0:C}").WithStyle(Styles.RightAlignedTextBlock)) _
                .AddBinding(9, 1, e.TotalDue.BindToTextBlock("{0:C}").WithStyle(Styles.RightAlignedTextBlock).AddBehavior(New TotalDueConditionalFormat(e.TotalDue))) _
                .AddBinding(2, 2, 8, 2, "Total: ".BindToLabel().WithStyle(Styles.Label).AdhereToFrozenRight()) _
                .AddBinding(9, 2, CalcTotalAmtFunc.BindToTextBlock("{0:C}").WithStyle(Styles.RightAlignedTextBlock).AddBehavior(New TotalAmtConditionalFormat(CalcTotalAmtFunc))) _
                .AddBehavior(New RowViewAlternation())
        End Sub

        Public Sub EnsureVisible(salesOrderId As System.Nullable(Of Integer))
            If Not salesOrderId.HasValue Then
                Return
            End If

            Dim current = GetRow(salesOrderId.Value)
            If current IsNot Nothing Then
                CurrentRow = current
                Scrollable.EnsureCurrentRowVisible()
            End If
        End Sub

        Private Function GetRow(salesOrderId As Integer) As RowPresenter
            Return Match(New SalesOrderHeader.PK(salesOrderId))
        End Function
    End Class
End Class
