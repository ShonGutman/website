using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Web.Model;

namespace Web.Pages
{
    public class AdminMenuModel : AuthorizedPageModel
    {
        private readonly Helper _helper;
        private readonly string _webRootPath;

        public List<User> Users { get; set; }
        public List<Purchase> Purchases { get; set; }
        public List<Item> Items { get; set; }
        public List<Purchase> SearchedPurchases { get; set; }
        public string SearchUsername { get; set; }
        public string StatusMessage { get; set; }

        public AdminMenuModel(IWebHostEnvironment webHostEnvironment)
        {
            _helper = new Helper();
            _webRootPath = webHostEnvironment.WebRootPath;
            Users = new List<User>();
            Purchases = new List<Purchase>();
            Items = new List<Item>();
        }

        public IActionResult OnGet()
        {
            // Check if user is authenticated and is an admin
            var authResult = EnsureAdmin();
            if (authResult != null)
            {
                return authResult;
            }

            // Load users, purchases, and items
            LoadUsers();
            LoadPurchases();
            LoadItems();

            return Page();
        }

        private void LoadUsers()
        {
            Users.Clear();
            var dt = _helper.RetrieveTable("SELECT * FROM Users", "Users");
            foreach (DataRow row in dt.Rows)
            {
                Users.Add(new User
                {
                    id = Convert.ToInt32(row["id"]),
                    first_name = row["first_name"].ToString(),
                    last_name = row["last_name"].ToString(),
                    username = row["username"].ToString(),
                    email = row["email"].ToString(),
                    is_admin = Convert.ToBoolean(row["is_admin"]),
                    date_of_birth = Convert.ToDateTime(row["date_of_birth"]),
                    phone_number = row["phone_number"].ToString(),
                    sex = row["sex"].ToString(),
                    address = row["address"].ToString()
                });
            }
        }

        private void LoadPurchases()
        {
            Purchases = _helper.GetAllPurchases();
        }

        private void LoadItems()
        {
            Items.Clear();
            var dt = _helper.GetAllItems();
            foreach (DataRow row in dt.Rows)
            {
                Items.Add(new Item
                {
                    Id = Convert.ToInt32(row["id"]),
                    name = row["name"].ToString(),
                    stock = Convert.ToInt32(row["stock"]),
                    price = Convert.ToDecimal(row["price"]),
                    image_path = row["image_path"].ToString()
                });
            }
        }

        public IActionResult OnPostRemoveUser(int userId)
        {
            // Check if user is authenticated and is an admin
            var authResult = EnsureAdmin();
            if (authResult != null)
            {
                return authResult;
            }

            try
            {
                _helper.RemoveUser(userId);
                StatusMessage = "User removed successfully.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error removing user: {ex.Message}";
            }

            // Reload all data
            LoadUsers();
            LoadPurchases();
            LoadItems();

            return Page();
        }

        public IActionResult OnPostSearchPurchases(string username)
        {
            // Check if user is authenticated and is an admin
            var authResult = EnsureAdmin();
            if (authResult != null)
            {
                return authResult;
            }

            SearchUsername = username;
            try
            {
                SearchedPurchases = _helper.GetPurchasesByUsername(username);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error searching purchases: {ex.Message}";
                SearchedPurchases = new List<Purchase>();
            }

            // Load all data
            LoadUsers();
            LoadPurchases();
            LoadItems();

            return Page();
        }

        public async Task<IActionResult> OnPostAddItem(string name, int stock, decimal price, IFormFile image)
        {
            // Check if user is authenticated and is an admin
            var authResult = EnsureAdmin();
            if (authResult != null)
            {
                return authResult;
            }

            try
            {
                // Process and save the image
                string imagePath = "images/default-item.jpg"; // Default image path
                if (image != null && image.Length > 0)
                {
                    // Create unique filename
                    string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                    string filePath = Path.Combine(_webRootPath, "images", fileName);

                    // Ensure directory exists
                    Directory.CreateDirectory(Path.Combine(_webRootPath, "images"));

                    // Save the file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    imagePath = $"images/{fileName}";
                }

                // Create item object
                var item = new Item
                {
                    name = name,
                    stock = stock,
                    price = price,
                    image_path = imagePath
                };

                // Save to database
                _helper.AddItem(item);
                StatusMessage = "Item added successfully.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error adding item: {ex.Message}";
            }

            // Reload all data
            LoadUsers();
            LoadPurchases();
            LoadItems();

            return Page();
        }

        public IActionResult OnPostRemoveItem(int itemId)
        {
            // Check if user is authenticated and is an admin
            var authResult = EnsureAdmin();
            if (authResult != null)
            {
                return authResult;
            }

            try
            {
                _helper.RemoveItem(itemId);
                StatusMessage = "Item removed successfully.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error removing item: {ex.Message}";
            }

            // Reload all data
            LoadUsers();
            LoadPurchases();
            LoadItems();

            return Page();
        }

        public IActionResult OnPostUpdateItemPrice(int itemId, decimal newPrice)
        {
            // Check if user is authenticated and is an admin
            var authResult = EnsureAdmin();
            if (authResult != null)
            {
                return authResult;
            }

            try
            {
                // Validate the new price
                if (newPrice <= 0)
                {
                    StatusMessage = "Price must be greater than zero.";
                }
                else
                {
                    // Call the helper method to update the price in the database
                    int rowsAffected = _helper.UpdateItemPrice(itemId, newPrice);

                    if (rowsAffected > 0)
                    {
                        StatusMessage = "Item price updated successfully.";
                    }
                    else
                    {
                        StatusMessage = "No item was found with the specified ID.";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating price: {ex.Message}";
            }

            // Reload all data
            LoadUsers();
            LoadPurchases();
            LoadItems();

            return Page();
        }

        public IActionResult OnPostUpdateItemStock(int itemId, int newStock)
        {
            // Check if user is authenticated and is an admin
            var authResult = EnsureAdmin();
            if (authResult != null)
            {
                return authResult;
            }

            try
            {
                // Validate the new stock value
                if (newStock < 0)
                {
                    StatusMessage = "Stock cannot be negative.";
                }
                else
                {
                    // Create a helper method to update the stock in the database
                    int rowsAffected = _helper.UpdateItemStock(itemId, newStock);

                    if (rowsAffected > 0)
                    {
                        StatusMessage = "Item stock updated successfully.";
                    }
                    else
                    {
                        StatusMessage = "No item was found with the specified ID.";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating stock: {ex.Message}";
            }

            // Reload all data
            LoadUsers();
            LoadPurchases();
            LoadItems();

            return Page();
        }
    }
}