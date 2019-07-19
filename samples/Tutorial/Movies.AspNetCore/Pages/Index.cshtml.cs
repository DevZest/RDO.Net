using DevZest.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Movies.AspNetCore.Pages
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

        public async Task OnGetAsync()
        {
            Movies = await _db.GetMoviesAsync(SearchString);
        }
    }
}
