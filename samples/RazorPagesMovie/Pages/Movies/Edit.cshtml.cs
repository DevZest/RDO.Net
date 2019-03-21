using System.Linq;
using System.Threading.Tasks;
using DevZest.Data;
using DevZest.Data.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.Movies
{
    public class EditModel : PageModel
    {
        private readonly Db _db;

        public EditModel(Db db)
        {
            _db = db;
        }

        [BindProperty]
        [Scalar]
        public DataSet<Movie> Movie { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            Movie = await _db.Movie.Where(m => m.ID == id).ToDataSetAsync();

            if (Movie.Count == 0)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            await _db.Movie.UpdateAsync(Movie);

            return RedirectToPage("./Index");
        }
    }
}
