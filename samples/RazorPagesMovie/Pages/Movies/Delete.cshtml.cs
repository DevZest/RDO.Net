using System.Collections.Generic;
using System.Threading.Tasks;
using DevZest.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.Movies
{
    public class DeleteModel : PageModel
    {
        private readonly Db _db;

        public DeleteModel(Db db)
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
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            await _db.Movie.DeleteAsync(m => m.ID == id);
            return RedirectToPage("./Index");
        }
    }
}
