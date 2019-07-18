Imports DevZest.Data.Presenters

Partial Class MovieDetailWindow
    Private NotInheritable Class Presenter
        Inherits DataPresenter(Of Movie)
        Private Const LABEL_FORMAT As String = "{0}:"

        Protected Overrides Sub BuildTemplate(builder As TemplateBuilder)
            Dim title = Entity.Title.BindToTextBox()
            Dim releaseDate = Entity.ReleaseDate.BindToDatePicker()
            Dim genre = Entity.Genre.BindToTextBox()
            Dim price = Entity.Price.BindToTextBox()

            builder.GridColumns("Auto", "*") _
                .GridRows("Auto", "Auto", "Auto", "Auto") _
                .AddBinding(0, 0, Entity.Title.BindToLabel(title, LABEL_FORMAT)) _
                .AddBinding(0, 1, Entity.ReleaseDate.BindToLabel(releaseDate, LABEL_FORMAT)) _
                .AddBinding(0, 2, Entity.Genre.BindToLabel(genre, LABEL_FORMAT)) _
                .AddBinding(0, 3, Entity.Price.BindToLabel(price, LABEL_FORMAT)) _
                .AddBinding(1, 0, title) _
                .AddBinding(1, 1, releaseDate) _
                .AddBinding(1, 2, genre) _
                .AddBinding(1, 3, price)
        End Sub

        Public ReadOnly Property ID() As Integer
            Get
                Return Entity.ID(0).Value
            End Get
        End Property

        Public ReadOnly Property IsNew() As Boolean
            Get
                Return ID < 1
            End Get
        End Property

        Public Function SaveToDbAsync() As Task
            If IsNew Then
                Return Application.ExecuteAsync(Function(db) db.Movie.InsertAsync(DataSet))
            Else
                Return Application.ExecuteAsync(Function(db) db.Movie.UpdateAsync(DataSet))
            End If
        End Function
    End Class
End Class
