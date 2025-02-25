using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Web.Pages
{
    public class RegisterModel : PageModel
    {
        [BindProperty, Required]
        public string FirstName { get; set; }

        [BindProperty, Required]
        public string LastName { get; set; }

        [BindProperty, Required]
        public string Username { get; set; }

        [BindProperty, Required, EmailAddress]
        public string Email { get; set; }

        [BindProperty, Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be 10 digits.")]
        public string PhoneNumber { get; set; }

        [BindProperty, Required]
        public string Sex { get; set; }

        [BindProperty, Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; } = new DateTime(2000, 1, 1); // Default date

        [BindProperty, Required]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must be at least 8 characters long and include at least one letter, one number, and one special character.")]
        public string Password { get; set; }

        [BindProperty, Required]
        public string Address { get; set; }

        [BindProperty, Required]
        [Display(Name = "I agree to the rules")]
        public bool AgreeToRules { get; set; }

        public void OnGet()
        {
            // This method handles the GET request (when the page is loaded)
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                // If validation fails, return the page with the current input values
                return Page();
            }

            // TODO: Save the user or perform other actions

            return RedirectToPage("/Index");
        }
    }
}