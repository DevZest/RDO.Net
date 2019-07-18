Imports DevZest.Data

Partial Class MainWindow
    Implements MainWindow.Presenter.IFilter

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
    End Class

    Private ReadOnly _presenter As Presenter

    Public Sub New()
        InitializeComponent()
        InitializeCommandBindings()
        _presenter = New Presenter(Me)
        _presenter.ShowAsync(_dataView)
    End Sub

    Private Property Text() As String Implements Presenter.IFilter.Text
        Get
            Return _textBoxSearch.Text
        End Get
        Set
            _textBoxSearch.Text = Value
        End Set
    End Property

    Private Sub InitializeCommandBindings()
        CommandBindings.Add(New CommandBinding(Commands.[New], AddressOf [New]))
        CommandBindings.Add(New CommandBinding(Commands.Open, AddressOf Open, AddressOf CanOpen))
        CommandBindings.Add(New CommandBinding(Commands.Delete, AddressOf Delete, AddressOf CanDelete))
        CommandBindings.Add(New CommandBinding(Commands.Refresh, AddressOf Refresh, AddressOf CanRefresh))
    End Sub

    Private Sub ShowMovieDetailWindow(movie As DataSet(Of Movie))
        Debug.Assert(movie.Count = 1)
        Dim result = MovieDetailWindow.Show(movie, Me)
        If result Then
            Refresh(True)
        End If
    End Sub

    Private Sub [New](sender As Object, e As ExecutedRoutedEventArgs)
        Dim movie = DataSet(Of Movie).Create()
        movie.AddRow()
        ShowMovieDetailWindow(movie)
    End Sub

    Private ReadOnly Property Entity() As Movie
        Get
            Return _presenter.Entity
        End Get
    End Property

    Private Async Sub Open(sender As Object, e As ExecutedRoutedEventArgs)
        Dim ID = _presenter.CurrentRow.GetValue(Entity.ID).Value
        Dim movie = Await Application.ExecuteAsync(Function(db) db.GetMovieAsync(ID))
        ShowMovieDetailWindow(movie)
    End Sub

    Private Sub CanOpen(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = _presenter.CurrentRow IsNot Nothing
    End Sub

    Private Async Sub Delete(sender As Object, e As ExecutedRoutedEventArgs)
        Dim selectedRows = _presenter.SelectedRows
        Dim json = _presenter.DataSet.Filter(JsonFilter.PrimaryKeyOnly).ToJsonString(_presenter.SelectedDataRows, False)
        Dim keys = DataSet(Of Movie.Key).ParseJson(json)
        Await Application.ExecuteAsync(Function(db) db.Movie.DeleteAsync(keys, Function(s, t) s.Match(t)))
        Refresh(False)
    End Sub

    Private Sub CanDelete(sender As Object, e As CanExecuteRoutedEventArgs)
        Dim selectedRows = _presenter.SelectedRows
        e.CanExecute = selectedRows IsNot Nothing AndAlso selectedRows.Count > 0
    End Sub

    Private Sub Refresh(sender As Object, e As ExecutedRoutedEventArgs)
        Refresh(False)
    End Sub

    Private Sub Refresh(clearFilter As Boolean)
        _presenter.RefreshAsync(clearFilter)
    End Sub

    Private Sub CanRefresh(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = _presenter.DataSet IsNot Nothing
    End Sub
End Class
