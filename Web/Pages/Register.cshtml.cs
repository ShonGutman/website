using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Web.Model;
using System.Security.Cryptography;
using System.Text;
using System;
using Microsoft.Data.SqlClient;

namespace Web.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly Helper _helper;

        public RegisterModel()
        {
            _helper = new Helper(); // Initialize the Helper class
        }

        // Properties for form binding
        [BindProperty, Required]
        public string FirstName { get; set; }

        [BindProperty, Required]
        public string LastName { get; set; }

        [BindProperty, Required]
        public string Username { get; set; }

        [BindProperty, Required]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        [BindProperty, Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be 10 digits.")]
        public string PhoneNumber { get; set; }

        [BindProperty, Required]
        public string Sex { get; set; }

        [BindProperty, Required]
        [DataType(DataType.Date)]
        [MinimumAge(18, ErrorMessage = "You must be at least 18 years old to register.")]
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

        // Handles GET requests (when the page is loaded)
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

        // Handles POST requests (when the form is submitted)
        public IActionResult OnPost()
        {
            // Validate the form data
            if (!ModelState.IsValid)
            {
                // If validation fails, return the page with the current input values
                return Page();
            }

            // Check if the username is already taken
            if (_helper.IsUsernameTaken(Username))
            {
                ModelState.AddModelError("Username", "This username is already taken. Please choose a different one.");
                return Page();
            }

            // Hash the password before saving it to the database
            string hashedPassword = HashPassword(Password);

            // Create a new User object with the form data
            var newUser = new User
            {
                first_name = FirstName,
                last_name = LastName,
                username = Username,
                password = hashedPassword, // Store the hashed password
                is_admin = false, // Default to false for new users
                date_of_birth = DateOfBirth,
                email = Email,
                phone_number = PhoneNumber,
                sex = Sex,
                address = Address
            };

            try
            {
                // Add the user to the database
                _helper.AddUser(newUser);

                // Redirect to the home page after successful registration
                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                // If an error occurs, add an error message to the ModelState
                ModelState.AddModelError(string.Empty, "An error occurred while registering. Please try again.");

                // Return the page with the error message
                return Page();
            }
        }

        // Helper method to hash the password using SHA-256
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                // Compute the hash of the password
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert the byte array to a hexadecimal string
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }

    // Custom validation attribute to ensure the user is at least 18 years old
    public class MinimumAgeAttribute : ValidationAttribute
    {
        private readonly int _minimumAge;

        public MinimumAgeAttribute(int minimumAge)
        {
            _minimumAge = minimumAge;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime dateOfBirth)
            {
                var today = DateTime.Today;
                var age = today.Year - dateOfBirth.Year;

                // Adjust age if the birthday hasn't occurred yet this year
                if (dateOfBirth.Date > today.AddYears(-age))
                {
                    age--;
                }

                if (age < _minimumAge)
                {
                    return new ValidationResult(ErrorMessage ?? $"You must be at least {_minimumAge} years old.");
                }
            }

            return ValidationResult.Success;
        }
    }
}