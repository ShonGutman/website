using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages
{
    public class AuthorizedPageModel : PageModel
    {
        // Require login for this page
        protected IActionResult EnsureAuthenticated()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToPage("/Login");
            }
            return null;
        }

        // Require admin privileges for this page
        protected IActionResult EnsureAdmin()
        {
            var isAdminString = HttpContext.Session.GetString("IsAdmin");
            if (HttpContext.Session.GetString("Username") == null ||
                string.IsNullOrEmpty(isAdminString) ||
                !bool.TryParse(isAdminString, out bool isAdmin) ||
                !isAdmin)
            {
                return RedirectToPage("/Login");
            }
            return null;
        }

        protected int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        protected string GetCurrentUsername()
        {
            return HttpContext.Session.GetString("Username") ?? string.Empty;
        }

        protected bool IsCurrentUserAdmin()
        {
            return HttpContext.Session.GetInt32("IsAdmin") == 1;
        }
    }
}