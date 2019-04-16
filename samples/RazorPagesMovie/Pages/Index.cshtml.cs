using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPagesMovie.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            return Redirect("/Movies");
        }
    }
}
