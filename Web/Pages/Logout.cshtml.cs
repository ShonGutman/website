using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace Web.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Clear the session
            HttpContext.Session.Clear();

            // Redirect to home page after logging out
            return RedirectToPage("/Index");
        }
    }
}
