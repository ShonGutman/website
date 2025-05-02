using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Web.Model;

namespace Web.Pages
{
    public class MenuModel : AuthorizedPageModel
    {
        private readonly Helper _helper;

        public List<Item> Items { get; set; }
        public string SearchTerm { get; set; }
        public Dictionary<int, int> CartItems { get; set; } = new Dictionary<int, int>();
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public MenuModel()
        {
            _helper = new Helper();
        }

        public IActionResult OnGet(string searchTerm, string errorMessage = null, string successMessage = null)
        {
            // Check if user is authenticated
            var authResult = EnsureAuthenticated();
            if (authResult != null)
            {
                return authResult;
            }

            // Get search term and messages
            SearchTerm = searchTerm;
            ErrorMessage = errorMessage;
            SuccessMessage = successMessage;

            // Load shopping cart from session if it exists
            var cartJson = HttpContext.Session.GetString("ShoppingCart");
            if (!string.IsNullOrEmpty(cartJson))
            {
                CartItems = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(cartJson);
            }

            // Get all items from database
            DataTable itemsTable = _helper.GetAllItems();
            Items = new List<Item>();

            foreach (DataRow row in itemsTable.Rows)
            {
                var item = new Item
                {
                    Id = (int)row["Id"],
                    name = row["name"].ToString(),
                    stock = (int)row["stock"],
                    price = (decimal)row["price"],
                    image_path = row["image_path"].ToString()
                };

                // If there's a search term, only add matching items
                if (string.IsNullOrEmpty(SearchTerm) ||
                    item.name.Contains(SearchTerm, System.StringComparison.OrdinalIgnoreCase))
                {
                    Items.Add(item);
                }
            }

            return Page();
        }

        public IActionResult OnPostAddToCart(int itemId, int quantity)
        {
            // Check if user is authenticated
            var authResult = EnsureAuthenticated();
            if (authResult != null)
            {
                return authResult;
            }

            // Get the current item from database to check stock
            DataTable itemsTable = _helper.GetAllItems();
            int availableStock = 0;
            string itemName = "";

            foreach (DataRow row in itemsTable.Rows)
            {
                if ((int)row["Id"] == itemId)
                {
                    availableStock = (int)row["stock"];
                    itemName = row["name"].ToString();
                    break;
                }
            }

            // Load current cart or create new one
            var cartJson = HttpContext.Session.GetString("ShoppingCart");
            Dictionary<int, int> cart = new Dictionary<int, int>();

            if (!string.IsNullOrEmpty(cartJson))
            {
                cart = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(cartJson);
            }

            // Calculate current cart quantity for this item
            int currentCartQuantity = cart.ContainsKey(itemId) ? cart[itemId] : 0;

            // Check if adding the requested quantity would exceed available stock
            if (currentCartQuantity + quantity > availableStock)
            {
                // Return with error message
                return RedirectToPage("/Menu", new
                {
                    searchTerm = SearchTerm,
                    errorMessage = $"Cannot add {quantity} of '{itemName}' to cart. Only {availableStock - currentCartQuantity} more available."
                });
            }

            // Add or update item quantity
            if (cart.ContainsKey(itemId))
            {
                cart[itemId] += quantity;
            }
            else
            {
                cart.Add(itemId, quantity);
            }

            // Save cart back to session
            HttpContext.Session.SetString("ShoppingCart", System.Text.Json.JsonSerializer.Serialize(cart));

            // Redirect back to the menu page with success message
            return RedirectToPage("/Menu", new
            {
                searchTerm = SearchTerm,
                successMessage = $"Added {quantity} '{itemName}' to your cart."
            });
        }

        public IActionResult OnPostRemoveFromCart(int itemId)
        {
            // Check if user is authenticated
            var authResult = EnsureAuthenticated();
            if (authResult != null)
            {
                return authResult;
            }

            // Get item name for message
            string itemName = "";
            DataTable itemsTable = _helper.GetAllItems();
            foreach (DataRow row in itemsTable.Rows)
            {
                if ((int)row["Id"] == itemId)
                {
                    itemName = row["name"].ToString();
                    break;
                }
            }

            // Load current cart
            var cartJson = HttpContext.Session.GetString("ShoppingCart");
            if (string.IsNullOrEmpty(cartJson))
            {
                return RedirectToPage("/Menu", new { searchTerm = SearchTerm });
            }

            Dictionary<int, int> cart = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(cartJson);

            // Remove item from cart
            if (cart.ContainsKey(itemId))
            {
                int removedQuantity = cart[itemId];
                cart.Remove(itemId);

                // Save updated cart back to session
                HttpContext.Session.SetString("ShoppingCart", System.Text.Json.JsonSerializer.Serialize(cart));

                return RedirectToPage("/Menu", new
                {
                    searchTerm = SearchTerm,
                    successMessage = $"Removed {removedQuantity} '{itemName}' from your cart."
                });
            }

            return RedirectToPage("/Menu", new { searchTerm = SearchTerm });
        }

        public IActionResult OnPostUpdateCartQuantity(int itemId, int newQuantity)
        {
            // Check if user is authenticated
            var authResult = EnsureAuthenticated();
            if (authResult != null)
            {
                return authResult;
            }

            // Get the current item from database to check stock
            DataTable itemsTable = _helper.GetAllItems();
            int availableStock = 0;
            string itemName = "";

            foreach (DataRow row in itemsTable.Rows)
            {
                if ((int)row["Id"] == itemId)
                {
                    availableStock = (int)row["stock"];
                    itemName = row["name"].ToString();
                    break;
                }
            }

            // Validate new quantity
            if (newQuantity <= 0)
            {
                // If quantity is 0 or negative, remove item from cart
                return OnPostRemoveFromCart(itemId);
            }

            if (newQuantity > availableStock)
            {
                // Return with error message
                return RedirectToPage("/Menu", new
                {
                    searchTerm = SearchTerm,
                    errorMessage = $"Cannot update '{itemName}' quantity to {newQuantity}. Only {availableStock} available in stock."
                });
            }

            // Load current cart
            var cartJson = HttpContext.Session.GetString("ShoppingCart");
            if (string.IsNullOrEmpty(cartJson))
            {
                return RedirectToPage("/Menu", new { searchTerm = SearchTerm });
            }

            Dictionary<int, int> cart = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(cartJson);

            // Update item quantity
            if (cart.ContainsKey(itemId))
            {
                cart[itemId] = newQuantity;

                // Save updated cart back to session
                HttpContext.Session.SetString("ShoppingCart", System.Text.Json.JsonSerializer.Serialize(cart));

                return RedirectToPage("/Menu", new
                {
                    searchTerm = SearchTerm,
                    successMessage = $"Updated '{itemName}' quantity to {newQuantity}."
                });
            }

            return RedirectToPage("/Menu", new { searchTerm = SearchTerm });
        }

        public IActionResult OnPostProceedToCheckout()
        {
            // Check if user is authenticated
            var authResult = EnsureAuthenticated();
            if (authResult != null)
            {
                return authResult;
            }

            // Validate cart against current stock levels before proceeding
            var cartJson = HttpContext.Session.GetString("ShoppingCart");
            if (string.IsNullOrEmpty(cartJson))
            {
                return RedirectToPage("/Menu");
            }

            Dictionary<int, int> cart = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(cartJson);
            DataTable itemsTable = _helper.GetAllItems();

            // Check each cart item against available stock
            foreach (var cartItem in cart)
            {
                int itemId = cartItem.Key;
                int requestedQuantity = cartItem.Value;

                foreach (DataRow row in itemsTable.Rows)
                {
                    if ((int)row["Id"] == itemId)
                    {
                        int availableStock = (int)row["stock"];
                        string itemName = row["name"].ToString();

                        if (requestedQuantity > availableStock)
                        {
                            // Stock has changed since item was added to cart
                            return RedirectToPage("/Menu", new
                            {
                                errorMessage = $"'{itemName}' has insufficient stock. Available: {availableStock}, In cart: {requestedQuantity}"
                            });
                        }
                        break;
                    }
                }
            }

            // Redirect to the checkout page
            return RedirectToPage("/Checkout");
        }
    }
}