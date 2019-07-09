Public Enum SearchBoxState
    Empty
    Search
    ClearSearch
End Enum

Public Class SearchBox
    Public NotInheritable Class Commands
        Public Shared ReadOnly Search As New RoutedUICommand()
        Public Shared ReadOnly ClearSearch As New RoutedUICommand()
    End Class

    Private Shared ReadOnly StatePropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(NameOf(State), GetType(SearchBoxState), GetType(SearchBox), New FrameworkPropertyMetadata(SearchBoxState.Empty))
    Public Shared ReadOnly StateProperty As DependencyProperty = StatePropertyKey.DependencyProperty

    Public Property State() As SearchBoxState
        Get
            Return CType(GetValue(StateProperty), SearchBoxState)
        End Get
        Private Set
            SetValue(StatePropertyKey, Value)
        End Set
    End Property

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Property SearchText() As String

    Private Sub SearchBox_GotFocus(sender As Object, e As RoutedEventArgs)
        Dim t As TextBox = CType(sender, TextBox)
        t.SelectAll()
    End Sub

    Private Sub SearchTextBox_GotMouseCapture(sender As Object, e As MouseEventArgs)
        Dim t As TextBox = CType(sender, TextBox)
        t.SelectAll()
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        If State = SearchBoxState.Search Then
            ExecuteSearchCommand()
        ElseIf State = SearchBoxState.ClearSearch Then
            ExecuteClearSearchCommand()
        End If
    End Sub

    Private Sub SearchTextBox_KeyDown(sender As Object, e As KeyEventArgs)
        If e.Key.Equals(Key.Enter) Then
            If State = SearchBoxState.Search Then
                e.Handled = ExecuteSearchCommand()
            End If
        ElseIf e.Key.Equals(Key.Escape) Then
            InputText = SearchText
            _searchTextBox.SelectAll()
        End If
    End Sub

    Private Function ExecuteSearchCommand() As Boolean
        If Commands.Search.CanExecute(Nothing, Nothing) Then
            SearchText = InputText
            Commands.Search.Execute(Nothing, Nothing)
            RefreshState()
            RestoreKeyboardFocus()
            Return True
        End If
        Return False
    End Function

    Private Function ExecuteClearSearchCommand() As Boolean
        If Commands.ClearSearch.CanExecute(Nothing, Nothing) Then
            SearchText = Nothing
            InputText = Nothing
            RefreshState()
            Commands.ClearSearch.Execute(Nothing, Nothing)
            RestoreKeyboardFocus()
            Return True
        End If
        Return False
    End Function

    Private Sub RestoreKeyboardFocus()
        ' This will set keyboard focus to top level Keyboard focus scope, normally the window's previously focused element.
        Keyboard.Focus(Nothing)
    End Sub

    Private Sub SearchTextBox_TextChanged(sender As Object, e As TextChangedEventArgs)
        RefreshState()
    End Sub

    Private Sub SearchTextBox_LostFocus(sender As Object, e As RoutedEventArgs)
        InputText = SearchText
    End Sub

    Private Property InputText() As String
        Get
            Return _searchTextBox.Text
        End Get
        Set
            _searchTextBox.Text = Value
        End Set
    End Property

    Private Sub RefreshState()
        State = CoerceState()
    End Sub

    Private Function CoerceState() As SearchBoxState
        If String.IsNullOrEmpty(InputText) Then
            Return SearchBoxState.Empty
        ElseIf InputText <> SearchText Then
            Return SearchBoxState.Search
        Else
            Return SearchBoxState.ClearSearch
        End If
    End Function
End Class
