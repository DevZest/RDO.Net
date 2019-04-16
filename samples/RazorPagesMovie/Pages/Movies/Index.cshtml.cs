using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevZest.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RazorPagesMovie.Pages.Movies
{
    public class IndexModel : PageModel
    {
        private readonly Db _db;

        public IndexModel(Db db)
        {
            _db = db;
        }

        public DataSet<Movie> Movies { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        public SelectList Genres { get; set; }

        [BindProperty(SupportsGet = true)]
        public string MovieGenre { get; set; }

        public async Task OnGetAsync()
        {
            var genres = await _db.CreateAggregateQuery((DbAggregateQueryBuilder builder, Adhoc adhoc) =>
            {
                builder.From(_db.Movie, out var m)
                    .Select(m.Genre, adhoc, nameof(Movie.Genre))
                    .GroupBy(m.Genre)
                    .OrderBy(m.Genre);
            }).ToDataSetAsync();

            Genres = new SelectList(GetGenres(genres));

            DbSet<Movie> movies = _db.Movie;

            if (!string.IsNullOrEmpty(SearchString))
                movies = movies.Where(s => s.Title.Contains(SearchString));

            if (!string.IsNullOrEmpty(MovieGenre))
                movies = movies.Where(x => x.Genre == MovieGenre);

            Movies = await movies.ToDataSetAsync();
        }

        private IEnumerable<string> GetGenres(DataSet<Adhoc> genres)
        {
            var genre = (_String)genres._[0];

            foreach (var dataRow in genres)
            {
                yield return genre[dataRow];
            }
        }
    }
}
