using DevZest.Data;
using DevZest.Data.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Movies.AspNetCore.Pages
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

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Movie = await _db.GetMovieAsync(id);

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