using DevZest.Data;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Movies.WPF
{
    partial class MainWindow
    {
        public static class Styles
        {
            public static readonly StyleId CheckBox = new StyleId(typeof(MainWindow));
            public static readonly StyleId LeftAlignedTextBlock = new StyleId(typeof(MainWindow));
            public static readonly StyleId RightAlignedTextBlock = new StyleId(typeof(MainWindow));
        }

        public sealed class Presenter : DataPresenter<Movie>
        {
            public interface IFilter
            {
                string Text { get; set; }
            }

            public Presenter(IFilter filter)
            {
                _filter = filter;
            }

            private readonly IFilter _filter;

            private Task<DataSet<Movie>> LoadDataAsync(CancellationToken ct)
            {
                return App.ExecuteAsync(db => db.GetMoviesAsync(_filter.Text, ct));
            }

            public void ShowAsync(DataView dataView)
            {
                ShowAsync(dataView, LoadDataAsync);
            }

            public Task RefreshAsync(bool clearFiler)
            {
                if (clearFiler)
                    _filter.Text = null;
                return RefreshAsync(LoadDataAsync);
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder.GridColumns("20", "*", "Auto", "Auto", "Auto")
                .GridRows("Auto", "Auto")
                .Layout(Orientation.Vertical)
                .WithFrozenTop(1)
                .AddBinding(0, 0, this.BindToCheckBox().WithStyle(Styles.CheckBox))
                .AddBinding(1, 0, _.Title.BindToColumnHeader())
                .AddBinding(2, 0, _.ReleaseDate.BindToColumnHeader())
                .AddBinding(3, 0, _.Genre.BindToColumnHeader())
                .AddBinding(4, 0, _.Price.BindToColumnHeader())
                .AddBinding(0, 1, _.BindToCheckBox().WithStyle(Styles.CheckBox))
                .AddBinding(1, 1, _.Title.BindToTextBlockHyperlink(Commands.Open).WithStyle(Styles.LeftAlignedTextBlock))
                .AddBinding(2, 1, _.ReleaseDate.BindToTextBlock("{0:d}").WithStyle(Styles.RightAlignedTextBlock))
                .AddBinding(3, 1, _.Genre.BindToTextBlock().WithStyle(Styles.LeftAlignedTextBlock))
                .AddBinding(4, 1, _.Price.BindToTextBlock("{0:C}").WithStyle(Styles.RightAlignedTextBlock));
            }
        }
    }
}
