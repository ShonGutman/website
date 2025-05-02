using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Web.Model;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace Web.Pages
{
    public class LoginModel : PageModel
    {
        private readonly Helper _helper;
        public LoginModel()
        {
            _helper = new Helper();
        }

        [BindProperty, Required]
        public string Username { get; set; }

        [BindProperty, Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public IActionResult OnGet()
        {
            // Check if the user is already logged in, redirect if so
            if (HttpContext.Session.GetString("Username") != null)
            {
                // If user is logged in, redirect to the page they came from or default to home
                return RedirectToPage("/Menu");
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Hash the password
            string hashedPassword = HashPassword(Password);

            // Verify user credentials
            var user = _helper.VerifyUserCredentials(Username, hashedPassword);
            if (user != null)
            {
                // Store user information in session
                HttpContext.Session.SetString("Username", user.username);
                HttpContext.Session.SetString("IsAdmin", user.is_admin.ToString());

                // Redirect based on user role
                if (user.is_admin)
                {
                    return RedirectToPage("/AdminMenu");
                }
                else
                {
                    return RedirectToPage("/Menu");
                }
            }
            else
            {
                // Set error message in ViewData
                ViewData["LoginError"] = "Invalid username or password.";
                return Page();
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}