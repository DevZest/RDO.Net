Imports System.Threading
Imports DevZest.Data
Imports DevZest.Data.Presenters
Imports DevZest.Data.Views

Partial Class ProductLookupWindow
    Private NotInheritable Class Presenter
        Inherits DataPresenter(Of Product)
        Implements RowView.ICommandService

        Public Sub New(dataView As DataView, currentProductID As Integer?)
            currentProductID = currentProductID
            AddHandler dataView.Loaded, AddressOf OnDataViewLoaded
        End Sub

        Protected Overrides Sub BuildTemplate(builder As TemplateBuilder)
            Dim e = Entity
            builder.GridColumns("Auto", "*", "Auto") _
                .GridRows("Auto", "20") _
                .RowView(Of RowView)(RowView.Styles.Selectable) _
                .Layout(Orientation.Vertical) _
                .WithFrozenTop(1) _
                .GridLineX(New GridPoint(0, 1), 3) _
                .GridLineY(New GridPoint(1, 1), 1) _
                .GridLineY(New GridPoint(2, 1), 1) _
                .GridLineY(New GridPoint(3, 1), 1) _
                .WithSelectionMode(SelectionMode.[Single]) _
                .AddBinding(0, 0, e.ProductNumber.BindToColumnHeader("Product Number")) _
                .AddBinding(1, 0, e.Name.BindToColumnHeader("Name")) _
                .AddBinding(2, 0, e.ListPrice.BindToColumnHeader("List Price")) _
                .AddBinding(0, 1, e.ProductNumber.BindToTextBlock()) _
                .AddBinding(1, 1, e.Name.BindToTextBlock()) _
                .AddBinding(2, 1, e.ListPrice.BindToTextBlock("{0:C}").WithStyle(MainWindow.Styles.RightAlignedTextBlock))
        End Sub

        Private Function LoadDataAsync(ct As CancellationToken) As Task(Of DataSet(Of Product))
            Return App.ExecuteAsync(Function(db) db.Product.ToDataSetAsync(ct))
        End Function

        Public Property CurrentProductID() As System.Nullable(Of Integer)

        Private Sub OnDataViewLoaded(sender As Object, e As RoutedEventArgs)
            Dim dataView As DataView = CType(sender, DataView)
            RemoveHandler dataView.Loaded, AddressOf OnDataViewLoaded
            ShowAsync(dataView)
        End Sub

        Private Overloads Async Sub ShowAsync(dataView As DataView)
            Await ShowAsync(dataView, AddressOf LoadDataAsync)
            If View.DataLoadState = DataLoadState.Succeeded Then
                SelectCurrent()
            End If
        End Sub

        Private Sub SelectCurrent()
            [Select](CurrentProductID)
        End Sub

        Private Overloads Sub [Select](currentProductID As System.Nullable(Of Integer))
            If Not currentProductID.HasValue Then
                Return
            End If

            Dim current = GetRow(currentProductID.Value)
            If current IsNot Nothing Then
                [Select](current, SelectionMode.Single)
            End If
        End Sub

        Private Function GetRow(currentProductID As Integer) As RowPresenter
            Return Match(New Product.PK(currentProductID))
        End Function

        Public Overloads Async Sub RefreshAsync()
            Await RefreshAsync(AddressOf LoadDataAsync)
            SelectCurrent()
        End Sub

        Private _searchText As String
        Public Property SearchText() As String
            Get
                Return _searchText
            End Get
            Set
                If _searchText = Value Then
                    Return
                End If

                _searchText = Value
                If String.IsNullOrEmpty(Value) Then
                    Where = Nothing
                Else
                    Where = Function(dataRow) Entity.ProductNumber(dataRow).Contains(Value) OrElse Entity.Name(dataRow).Contains(Value)
                End If
                SelectCurrent()
            End Set
        End Property

        Iterator Function GetCommandEntries(rowView As RowView) As IEnumerable(Of CommandEntry) Implements RowView.ICommandService.GetCommandEntries
            Dim baseService = Me.GetRegisteredService(Of RowView.ICommandService)()
            For Each entry In baseService.GetCommandEntries(rowView)
                Yield entry
            Next
            Yield Commands.SelectCurrent.Bind(New KeyGesture(Key.Enter), New MouseGesture(MouseAction.LeftDoubleClick))
        End Function
    End Class

End Class
