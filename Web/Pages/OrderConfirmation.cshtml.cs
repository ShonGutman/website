using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using Web.Model;

namespace Web.Pages
{
    public class OrderConfirmationModel : AuthorizedPageModel
    {
        private readonly Helper _helper;

        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string UserEmail { get; set; }

        public OrderConfirmationModel()
        {
            _helper = new Helper();
        }

        public IActionResult OnGet(int orderId)
        {
            // Check if user is authenticated
            var authResult = EnsureAuthenticated();
            if (authResult != null)
            {
                return authResult;
            }

            // Set order ID
            OrderId = orderId;

            // Get order information from database
            string sqlQuery = @"
                SELECT p.purchase_date, u.email
                FROM Purchases p
                JOIN Users u ON p.user_id = u.id
                WHERE p.id = @OrderId";

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "@OrderId", OrderId }
            };

            DataTable orderTable = _helper.RetrieveTableWithParams(sqlQuery, "OrderDetails", parameters);
            if (orderTable.Rows.Count > 0)
            {
                OrderDate = (DateTime)orderTable.Rows[0]["purchase_date"];
                UserEmail = orderTable.Rows[0]["email"].ToString();
            }
            else
            {
                // Order not found, redirect to menu
                return RedirectToPage("/Menu");
            }

            return Page();
        }
    }
}