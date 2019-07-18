using DevZest.Data.Presenters;
using System.Threading.Tasks;

namespace Movies.WPF
{
    partial class MovieDetailWindow
    {
        private sealed class Presenter : DataPresenter<Movie>
        {
            private const string LABEL_FORMAT = "{0}:";

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                var title = _.Title.BindToTextBox();
                var releaseDate = _.ReleaseDate.BindToDatePicker();
                var genre = _.Genre.BindToTextBox();
                var price = _.Price.BindToTextBox();

                builder
                    .GridColumns("Auto", "*")
                    .GridRows("Auto", "Auto", "Auto", "Auto")
                    .AddBinding(0, 0, _.Title.BindToLabel(title, LABEL_FORMAT))
                    .AddBinding(0, 1, _.ReleaseDate.BindToLabel(releaseDate, LABEL_FORMAT))
                    .AddBinding(0, 2, _.Genre.BindToLabel(genre, LABEL_FORMAT))
                    .AddBinding(0, 3, _.Price.BindToLabel(price, LABEL_FORMAT))
                    .AddBinding(1, 0, title)
                    .AddBinding(1, 1, releaseDate)
                    .AddBinding(1, 2, genre)
                    .AddBinding(1, 3, price);
            }

            public int ID
            {
                get { return _.ID[0].Value; }
            }

            public bool IsNew
            {
                get { return ID < 1; }
            }

            public Task SaveToDbAsync()
            {
                if (IsNew)
                    return App.ExecuteAsync(db => db.Movie.InsertAsync(DataSet));
                else
                    return App.ExecuteAsync(db => db.Movie.UpdateAsync(DataSet));
            }
        }
    }
}
