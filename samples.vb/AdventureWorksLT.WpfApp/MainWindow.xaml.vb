Imports DevZest.Data

Partial Class MainWindow
    Public NotInheritable Class Commands
        Private Sub New()
        End Sub

        Public Shared ReadOnly Property [New]() As RoutedUICommand
            Get
                Return ApplicationCommands.[New]
            End Get
        End Property

        Public Shared ReadOnly Property Open() As RoutedUICommand
            Get
                Return ApplicationCommands.Open
            End Get
        End Property

        Public Shared ReadOnly Property Delete() As RoutedUICommand
            Get
                Return ApplicationCommands.Delete
            End Get
        End Property

        Public Shared ReadOnly Property Refresh() As RoutedUICommand
            Get
                Return NavigationCommands.Refresh
            End Get
        End Property

        Public Shared ReadOnly Property Search() As RoutedUICommand
            Get
                Return SearchBox.Commands.Search
            End Get
        End Property

        Public Shared ReadOnly Property ClearSearch() As RoutedUICommand
            Get
                Return SearchBox.Commands.ClearSearch
            End Get
        End Property

        Public Shared ReadOnly Property Close() As RoutedUICommand
            Get
                Return ApplicationCommands.Close
            End Get
        End Property
    End Class

    Private ReadOnly _presenter As Presenter

    Public Sub New()
        InitializeComponent()
        InitializeCommandBindings()
        _presenter = New Presenter()
        _presenter.ShowAsync(_dataView)
    End Sub

    Private Sub InitializeCommandBindings()
        CommandBindings.Add(New CommandBinding(Commands.[New], AddressOf [New]))
        CommandBindings.Add(New CommandBinding(Commands.Open, AddressOf Open, AddressOf CanOpen))
        CommandBindings.Add(New CommandBinding(Commands.Delete, AddressOf Delete, AddressOf CanDelete))
        CommandBindings.Add(New CommandBinding(Commands.Refresh, AddressOf Refresh, AddressOf CanRefresh))
        CommandBindings.Add(New CommandBinding(Commands.Search, AddressOf Search, AddressOf CanRefresh))
        CommandBindings.Add(New CommandBinding(Commands.ClearSearch, AddressOf ClearSearch, AddressOf CanRefresh))
        CommandBindings.Add(New CommandBinding(Commands.Close, AddressOf Close))
    End Sub

    Private ReadOnly Property Entity() As SalesOrderHeader
        Get
            Return _presenter.Entity
        End Get
    End Property

    Private Sub [New](sender As Object, e As ExecutedRoutedEventArgs)
        Dim salesOrderInfo = DataSet(Of SalesOrderInfo).Create()
        salesOrderInfo.AddRow()
        Dim window = New SalesOrderWindow()
        window.Show(salesOrderInfo, Me, AddressOf Refresh)
    End Sub

    Private Sub Open(sender As Object, e As ExecutedRoutedEventArgs)
        Dim salesOrderID = _presenter.CurrentRow.GetValue(Entity.SalesOrderID).Value
        Dim dataSet As DataSet(Of SalesOrderInfo) = Nothing
        If App.Execute(Function(db, ct) db.GetSalesOrderInfoAsync(salesOrderID, ct), Me, dataSet) Then
            If dataSet.Count = 1 Then
                Dim window = New SalesOrderWindow()
                window.Show(dataSet, Me, AddressOf Refresh)
            Else
                MessageBox.Show("No data returned from server!")
            End If
        End If
    End Sub

    Private Async Sub Refresh(salesOrderId As System.Nullable(Of Integer))
        Await _presenter.RefreshAsync()
        _presenter.EnsureVisible(salesOrderId)
    End Sub

    Private Sub CanOpen(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = _presenter.CurrentRow IsNot Nothing
    End Sub

    Private Sub Delete(sender As Object, e As ExecutedRoutedEventArgs)
        Dim selectedRows = _presenter.SelectedRows
        Dim messageBoxText = String.Format("Are you sure you want to delete selected {0} rows?", selectedRows.Count)
        Dim caption = "Delete Sales Order(s)"
        If MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.No) = MessageBoxResult.No Then
            Return
        End If

        Dim keys = DataSet(Of SalesOrderHeader.Key).ParseJson(_presenter.DataSet.Filter(JsonFilter.PrimaryKeyOnly).ToJsonString(_presenter.SelectedDataRows, False))
        Dim success = App.Execute(Function(db, ct) db.DeleteSalesOrderAsync(keys, ct), Me, caption)
        If success Then
            RefreshList()
        End If
    End Sub

    Private Sub CanDelete(sender As Object, e As CanExecuteRoutedEventArgs)
        Dim selectedRows = _presenter.SelectedRows
        e.CanExecute = selectedRows IsNot Nothing AndAlso selectedRows.Count > 0
    End Sub

    Private Sub Refresh(sender As Object, e As ExecutedRoutedEventArgs)
        RefreshList()
    End Sub

    Private Sub RefreshList()
        _presenter.RefreshAsync()
    End Sub

    Private Sub CanRefresh(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = _presenter.DataSet IsNot Nothing
    End Sub

    Private Sub Search(sender As Object, e As ExecutedRoutedEventArgs)
        _presenter.SearchText = _searchBox.SearchText
    End Sub

    Private Sub ClearSearch(sender As Object, e As ExecutedRoutedEventArgs)
        _presenter.SearchText = Nothing
    End Sub

    Private Overloads Sub Close(sender As Object, e As ExecutedRoutedEventArgs)
        Me.Close()
    End Sub
End Class
