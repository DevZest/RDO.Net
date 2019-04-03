using System.Threading.Tasks;
using DevZest.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.Movies
{
    public class DetailsModel : PageModel
    {
        private readonly Db _db;

        public DetailsModel(Db db)
        {
            _db = db;
        }

        public DataSet<Movie> Movie { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Movie = await _db.Movie.Where(m => m.ID == id).ToDataSetAsync();

            if (Movie.Count == 0)
                return NotFound();

            return Page();
        }
    }
}
