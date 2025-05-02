using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using Web.Model;

namespace Web.Pages
{
    public class CheckoutModel : AuthorizedPageModel
    {
        private readonly Helper _helper;

        public Dictionary<int, int> CartItems { get; set; } = new Dictionary<int, int>();
        public List<Item> Items { get; set; } = new List<Item>();
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
        public decimal Subtotal { get; set; } = 0;
        public decimal Total { get; set; } = 0;
        public string Username { get; set; }

        public CheckoutModel()
        {
            _helper = new Helper();
        }

        public IActionResult OnGet(string errorMessage = null, string successMessage = null)
        {
            // Check if user is authenticated
            var authResult = EnsureAuthenticated();
            if (authResult != null)
            {
                return authResult;
            }

            // Get username from session
            Username = HttpContext.Session.GetString("Username");

            // Set messages
            ErrorMessage = errorMessage;
            SuccessMessage = successMessage;

            // Load shopping cart from session
            LoadCartAndItems();

            // Calculate totals
            CalculateTotals();

            return Page();
        }

        public IActionResult OnPostCompleteOrder(string cardNumber, string cvv, string expiryDate, string cardholderName)
        {
            // Check if user is authenticated
            var authResult = EnsureAuthenticated();
            if (authResult != null)
            {
                return authResult;
            }

            // Validate credit card info
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length != 16 || !cardNumber.All(char.IsDigit))
            {
                return RedirectToPage("/Checkout", new { errorMessage = "Invalid credit card number. Must be 16 digits." });
            }

            if (string.IsNullOrEmpty(cvv) || cvv.Length != 3 || !cvv.All(char.IsDigit))
            {
                return RedirectToPage("/Checkout", new { errorMessage = "Invalid CVV. Must be 3 digits." });
            }

            if (string.IsNullOrEmpty(expiryDate) || string.IsNullOrEmpty(cardholderName))
            {
                return RedirectToPage("/Checkout", new { errorMessage = "All payment fields are required." });
            }

            // Load cart and items
            LoadCartAndItems();

            // If cart is empty, redirect back
            if (CartItems.Count == 0)
            {
                return RedirectToPage("/Checkout", new { errorMessage = "Your cart is empty." });
            }

            try
            {
                // Begin transaction
                // 1. Get current user ID
                int userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return RedirectToPage("/Checkout", new { errorMessage = "User session expired. Please login again." });
                }

                // 2. Create a new purchase record
                int purchaseId = CreatePurchaseRecord(userId);
                if (purchaseId <= 0)
                {
                    return RedirectToPage("/Checkout", new { errorMessage = "Failed to create purchase record." });
                }

                // 3. Add item purchases and update stock
                bool success = AddItemPurchasesAndUpdateStock(purchaseId);
                if (!success)
                {
                    return RedirectToPage("/Checkout", new { errorMessage = "Failed to process order items." });
                }

                // 4. Clear the cart
                HttpContext.Session.Remove("ShoppingCart");

                // Redirect to order confirmation page
                return RedirectToPage("/OrderConfirmation", new { orderId = purchaseId });
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Checkout", new { errorMessage = "An error occurred: " + ex.Message });
            }
        }

        // Helper methods
        private void LoadCartAndItems()
        {
            // Load shopping cart from session if it exists
            var cartJson = HttpContext.Session.GetString("ShoppingCart");
            if (!string.IsNullOrEmpty(cartJson))
            {
                CartItems = JsonSerializer.Deserialize<Dictionary<int, int>>(cartJson);
            }

            // Get all items from database
            DataTable itemsTable = _helper.GetAllItems();
            Items = new List<Item>();

            foreach (DataRow row in itemsTable.Rows)
            {
                Items.Add(new Item
                {
                    Id = (int)row["Id"],
                    name = row["name"].ToString(),
                    stock = (int)row["stock"],
                    price = (decimal)row["price"],
                    image_path = row["image_path"].ToString()
                });
            }
        }

        private void CalculateTotals()
        {
            // Calculate subtotal
            Subtotal = 0;
            foreach (var cartItem in CartItems)
            {
                var item = Items.FirstOrDefault(i => i.Id == cartItem.Key);
                if (item != null)
                {
                    Subtotal += item.price * cartItem.Value;
                }
            }

            // Total is the same as subtotal since there's no discount
            Total = Subtotal;
        }

        private int GetCurrentUserId()
        {
            string username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return -1;
            }

            // Get user ID from database using username
            string sqlQuery = "SELECT id FROM Users WHERE username = @Username";
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "@Username", username }
            };

            DataTable userTable = _helper.RetrieveTableWithParams(sqlQuery, "Users", parameters);
            if (userTable.Rows.Count > 0)
            {
                return (int)userTable.Rows[0]["id"];
            }

            return -1;
        }

        private int CreatePurchaseRecord(int userId)
        {
            // Create SQL query to insert purchase and get the ID
            string sqlInsert = @"
                INSERT INTO Purchases (user_id, purchase_date)
                VALUES (@UserId, @PurchaseDate);
                SELECT SCOPE_IDENTITY();";

            using (var con = new Microsoft.Data.SqlClient.SqlConnection(_helper.GetConnectionString()))
            {
                con.Open();
                using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(sqlInsert, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@PurchaseDate", DateTime.Now);

                    // Execute and get the ID
                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }

            return -1;
        }

        private bool AddItemPurchasesAndUpdateStock(int purchaseId)
        {
            // Calculate totals first to ensure we have the correct prices
            CalculateTotals();

            using (var con = new Microsoft.Data.SqlClient.SqlConnection(_helper.GetConnectionString()))
            {
                con.Open();
                using (var transaction = con.BeginTransaction())
                {
                    try
                    {
                        foreach (var cartItem in CartItems)
                        {
                            int itemId = cartItem.Key;
                            int quantity = cartItem.Value;

                            // Get the current item
                            var item = Items.FirstOrDefault(i => i.Id == itemId);
                            if (item == null)
                            {
                                transaction.Rollback();
                                return false;
                            }

                            // Check stock one more time
                            if (item.stock < quantity)
                            {
                                transaction.Rollback();
                                return false;
                            }

                            // 1. Insert item purchase
                            string insertItemPurchase = @"
                                INSERT INTO ItemPurchases (purchase_id, item_id, quantity, item_price_at_purchase)
                                VALUES (@PurchaseId, @ItemId, @Quantity, @ItemPrice)";

                            using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(insertItemPurchase, con, transaction))
                            {
                                cmd.Parameters.AddWithValue("@PurchaseId", purchaseId);
                                cmd.Parameters.AddWithValue("@ItemId", itemId);
                                cmd.Parameters.AddWithValue("@Quantity", quantity);
                                cmd.Parameters.AddWithValue("@ItemPrice", item.price);
                                cmd.ExecuteNonQuery();
                            }

                            // 2. Update stock
                            string updateStock = @"
                                UPDATE Items
                                SET stock = stock - @Quantity
                                WHERE Id = @ItemId";

                            using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(updateStock, con, transaction))
                            {
                                cmd.Parameters.AddWithValue("@ItemId", itemId);
                                cmd.Parameters.AddWithValue("@Quantity", quantity);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // Commit the transaction
                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
