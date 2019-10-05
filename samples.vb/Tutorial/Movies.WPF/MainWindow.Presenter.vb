Imports System.Threading
Imports DevZest.Data
Imports DevZest.Data.Presenters
Imports DevZest.Data.Views

Partial Class MainWindow
    Public NotInheritable Class Styles
        Public Shared ReadOnly CheckBox As New StyleId(GetType(MainWindow))
        Public Shared ReadOnly LeftAlignedTextBlock As New StyleId(GetType(MainWindow))
        Public Shared ReadOnly RightAlignedTextBlock As New StyleId(GetType(MainWindow))
    End Class

    Public NotInheritable Class Presenter
        Inherits DataPresenter(Of Movie)

        Public Interface IFilter
            Property Text() As String
        End Interface

        Public Sub New(filter As IFilter)
            _filter = filter
        End Sub

        Private ReadOnly _filter As IFilter

        Private Function LoadDataAsync(ct As CancellationToken) As Task(Of DataSet(Of Movie))
            Return Application.ExecuteAsync(Function(db) db.GetMoviesAsync(_filter.Text, ct))
        End Function

        Public Overloads Sub ShowAsync(dataView As DataView)
            ShowAsync(dataView, AddressOf LoadDataAsync)
        End Sub

        Public Overloads Function RefreshAsync(clearFilder As Boolean) As Task
            If clearFilder Then
                _filter.Text = Nothing
            End If
            Return RefreshAsync(AddressOf LoadDataAsync)
        End Function

        Protected Overrides Sub BuildTemplate(builder As TemplateBuilder)
            builder.GridColumns("20", "*", "Auto", "Auto", "Auto") _
                .GridRows("Auto", "Auto") _
                .Layout(Orientation.Vertical) _
                .WithFrozenTop(1) _
                .AddBinding(0, 0, Me.BindToCheckBox().WithStyle(Styles.CheckBox)) _
                .AddBinding(1, 0, Entity.Title.BindToColumnHeader()) _
                .AddBinding(2, 0, Entity.ReleaseDate.BindToColumnHeader()) _
                .AddBinding(3, 0, Entity.Genre.BindToColumnHeader()) _
                .AddBinding(4, 0, Entity.Price.BindToColumnHeader()) _
                .AddBinding(0, 1, Entity.BindToCheckBox().WithStyle(Styles.CheckBox)) _
                .AddBinding(1, 1, Entity.Title.BindToHyperlink(Commands.Open).WithStyle(Styles.LeftAlignedTextBlock)) _
                .AddBinding(2, 1, Entity.ReleaseDate.BindToTextBlock("{0:d}").WithStyle(Styles.RightAlignedTextBlock)) _
                .AddBinding(3, 1, Entity.Genre.BindToTextBlock().WithStyle(Styles.LeftAlignedTextBlock)) _
                .AddBinding(4, 1, Entity.Price.BindToTextBlock("{0:C}").WithStyle(Styles.RightAlignedTextBlock))
        End Sub
    End Class
End Class
