using DevZest.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Movies
{
    partial class Db
    {
        public Task<DataSet<Movie>> GetMoviesAsync(string text, CancellationToken ct = default(CancellationToken))
        {
            DbSet<Movie> result = Movie;

            if (!string.IsNullOrWhiteSpace(text))
                result = Filter(result, text);

            return result.ToDataSetAsync(ct);
        }

        private static DbSet<Movie> Filter(DbSet<Movie> movies, _String text)
        {
            return movies.Where(_ => _.Title.Contains(text) | _.Genre.Contains(text));
        }

        public Task<DataSet<Movie>> GetMovieAsync(_Int32 id, CancellationToken ct = default(CancellationToken))
        {
            return Movie.Where(_ => _.ID == id).ToDataSetAsync(ct);
        }
    }
}
