using System.Threading.Tasks;
using DevZest.Data;
using DevZest.Data.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPagesMovie.Pages.Movies
{
    public class CreateModel : PageModel
    {
        private readonly Db _db;

        public CreateModel(Db db)
        {
            _db = db;
            Movie = DataSet<Movie>.Create();
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        [Scalar]
        public DataSet<Movie> Movie { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            await _db.Movie.InsertAsync(Movie);

            return RedirectToPage("./Index");
        }
    }
}