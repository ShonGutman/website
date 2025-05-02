using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using Web.Model;

namespace Web.Pages
{
    public class OrderHistoryModel : AuthorizedPageModel
    {
        private readonly Helper _helper;

        public List<Purchase> Orders { get; set; } = new List<Purchase>();

        public OrderHistoryModel()
        {
            _helper = new Helper();
        }

        public IActionResult OnGet()
        {
            // Check if user is authenticated
            var authResult = EnsureAuthenticated();
            if (authResult != null)
            {
                return authResult;
            }

            // Get username from session
            string username = HttpContext.Session.GetString("Username");

            // Retrieve orders for the user
            Orders = _helper.GetPurchasesByUsername(username);

            return Page();
        }
    }
}
