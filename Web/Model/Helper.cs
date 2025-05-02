using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace Web.Model
{
    public class Helper
    {
        private string conString;

        public Helper()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            conString = configuration.GetConnectionString("UsersDB");
        }

        // Retrieve a table from the database
        public DataTable RetrieveTable(string SQLStr, string table)
        {
            using (SqlConnection con = new SqlConnection(conString))
            {
                SqlCommand cmd = new SqlCommand(SQLStr, con);
                SqlDataAdapter ad = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                ad.Fill(ds, table);
                return ds.Tables[table];
            }
        }

        // Retrieve a table with parameters
        public DataTable RetrieveTableWithParams(string SQLStr, string table, Dictionary<string, object> parameters)
        {
            using (SqlConnection con = new SqlConnection(conString))
            {
                SqlCommand cmd = new SqlCommand(SQLStr, con);

                // Add parameters to the command
                foreach (var param in parameters)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }

                SqlDataAdapter ad = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                ad.Fill(ds, table);
                return ds.Tables[table];
            }
        }

        public string GetConnectionString()
        {
            return conString;
        }

        // Add a new user to the database
        public int AddUser(User user)
        {
            string sqlQuery = @"
                INSERT INTO Users
                (first_name, last_name, username, password, is_admin, date_of_birth, email, phone_number, sex, address)
                VALUES
                (@FirstName, @LastName, @Username, @Password, @IsAdmin, @DateOfBirth, @Email, @PhoneNumber, @Sex, @Address)";

            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    cmd.Parameters.AddWithValue("@FirstName", user.first_name);
                    cmd.Parameters.AddWithValue("@LastName", user.last_name);
                    cmd.Parameters.AddWithValue("@Username", user.username);
                    cmd.Parameters.AddWithValue("@Password", user.password); // Password should already be hashed
                    cmd.Parameters.AddWithValue("@IsAdmin", user.is_admin);
                    cmd.Parameters.AddWithValue("@DateOfBirth", user.date_of_birth);
                    cmd.Parameters.AddWithValue("@Email", user.email);
                    cmd.Parameters.AddWithValue("@PhoneNumber", user.phone_number);
                    cmd.Parameters.AddWithValue("@Sex", user.sex);
                    cmd.Parameters.AddWithValue("@Address", user.address);

                    return cmd.ExecuteNonQuery();
                }
            }
        }

        // Update an existing user in the database
        public int UpdateUser(User user)
        {
            string sqlQuery = @"
                UPDATE Users
                SET
                    first_name = @FirstName,
                    last_name = @LastName,
                    username = @Username,
                    password = @Password,
                    is_admin = @IsAdmin,
                    date_of_birth = @DateOfBirth,
                    email = @Email,
                    phone_number = @PhoneNumber,
                    sex = @Sex,
                    address = @Address
                WHERE id = @Id";

            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Id", user.id);
                    cmd.Parameters.AddWithValue("@FirstName", user.first_name);
                    cmd.Parameters.AddWithValue("@LastName", user.last_name);
                    cmd.Parameters.AddWithValue("@Username", user.username);
                    cmd.Parameters.AddWithValue("@Password", user.password); // Password should already be hashed
                    cmd.Parameters.AddWithValue("@IsAdmin", user.is_admin);
                    cmd.Parameters.AddWithValue("@DateOfBirth", user.date_of_birth);
                    cmd.Parameters.AddWithValue("@Email", user.email);
                    cmd.Parameters.AddWithValue("@PhoneNumber", user.phone_number);
                    cmd.Parameters.AddWithValue("@Sex", user.sex);
                    cmd.Parameters.AddWithValue("@Address", user.address);

                    return cmd.ExecuteNonQuery();
                }
            }
        }

        // Delete a user from the database
        public int DeleteUser(int userId)
        {
            string sqlQuery = "DELETE FROM Users WHERE id = @Id";

            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Id", userId);
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        // Retrieve a user by ID
        public User GetUserById(int userId)
        {
            string sqlQuery = "SELECT * FROM Users WHERE id = @Id";

            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Id", userId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                id = reader.GetInt32("id"),
                                first_name = reader.GetString("first_name"),
                                last_name = reader.GetString("last_name"),
                                username = reader.GetString("username"),
                                password = reader.GetString("password"),
                                is_admin = reader.GetBoolean("is_admin"),
                                date_of_birth = reader.GetDateTime("date_of_birth"),
                                email = reader.GetString("email"),
                                phone_number = reader.GetString("phone_number"),
                                sex = reader.GetString("sex"),
                                address = reader.GetString("address")
                            };
                        }
                    }
                }
            }
            return null; // Return null if no user is found
        }

        // Check if a username is already taken
        public bool IsUsernameTaken(string username)
        {
            string sqlQuery = "SELECT COUNT(*) FROM Users WHERE username = @Username";

            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0; // If count > 0, the username is taken
                }
            }
        }

        public User VerifyUserCredentials(string username, string hashedPassword)
        {
            string sqlQuery = "SELECT * FROM Users WHERE username = @Username AND password = @Password";

            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", hashedPassword);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                id = reader.GetInt32("id"),
                                first_name = reader.GetString("first_name"),
                                last_name = reader.GetString("last_name"),
                                username = reader.GetString("username"),
                                password = reader.GetString("password"),
                                is_admin = reader.GetBoolean("is_admin"),
                                date_of_birth = reader.GetDateTime("date_of_birth"),
                                email = reader.GetString("email"),
                                phone_number = reader.GetString("phone_number"),
                                sex = reader.GetString("sex"),
                                address = reader.GetString("address")
                            };
                        }
                    }
                }
            }
            return null; // Return null if no user is found
        }

        // Add a new purchase to the database
        public int AddPurchase(int userId)
        {
            string sqlQuery = @"
                INSERT INTO Purchases (user_id, purchase_date)
                VALUES (@UserId, @PurchaseDate)";

            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@PurchaseDate", DateTime.Now);

                    return cmd.ExecuteNonQuery();
                }
            }
        }

        // Add a new item to the database
        public int AddItem(Item item)
        {
            string sqlQuery = @"
                INSERT INTO Items (name, stock, price, image_path)
                VALUES (@Name, @Stock, @Price, @ImagePath)";

            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Name", item.name);
                    cmd.Parameters.AddWithValue("@Stock", item.stock);
                    cmd.Parameters.AddWithValue("@Price", item.price);
                    cmd.Parameters.AddWithValue("@ImagePath", item.image_path);

                    return cmd.ExecuteNonQuery();
                }
            }
        }

        // Add an item purchase to the database
        public int AddItemPurchase(int purchaseId, int itemId, int quantity, decimal itemPriceAtPurchase)
        {
            string sqlQuery = @"
                INSERT INTO ItemPurchases (purchase_id, item_id, quantity, item_price_at_purchase)
                VALUES (@PurchaseId, @ItemId, @Quantity, @ItemPriceAtPurchase)";

            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    cmd.Parameters.AddWithValue("@PurchaseId", purchaseId);
                    cmd.Parameters.AddWithValue("@ItemId", itemId);
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    cmd.Parameters.AddWithValue("@ItemPriceAtPurchase", itemPriceAtPurchase);

                    return cmd.ExecuteNonQuery();
                }
            }
        }

        // Retrieve all purchases with total price
        public List<Purchase> GetAllPurchases()
        {
            string sqlQuery = @"
                SELECT p.id, p.purchase_date, u.first_name, u.last_name, u.email, u.phone_number, u.address, u.sex,
                       ip.item_id, ip.quantity, ip.item_price_at_purchase, i.name as item_name
                FROM Purchases p
                JOIN Users u ON p.user_id = u.id
                JOIN ItemPurchases ip ON p.id = ip.purchase_id
                JOIN Items i ON ip.item_id = i.Id";

            var purchases = new List<Purchase>();

            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var purchase = purchases.FirstOrDefault(p => p.Id == reader.GetInt32("id"));
                            if (purchase == null)
                            {
                                purchase = new Purchase
                                {
                                    Id = reader.GetInt32("id"),
                                    PurchaseDate = reader.GetDateTime("purchase_date"),
                                    ItemPurchases = new List<ItemPurchase>()
                                };
                                purchases.Add(purchase);
                            }

                            purchase.ItemPurchases.Add(new ItemPurchase
                            {
                                ItemId = reader.GetInt32("item_id"),
                                Quantity = reader.GetInt32("quantity"),
                                ItemPriceAtPurchase = reader.GetDecimal("item_price_at_purchase"),
                                Item = new Item
                                {
                                    name = reader.GetString("item_name")
                                }
                            });
                        }
                    }
                }
            }

            return purchases;
        }

        // Retrieve all items
        public DataTable GetAllItems()
        {
            string sqlQuery = "SELECT * FROM Items";

            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    SqlDataAdapter ad = new SqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    ad.Fill(ds, "Items");
                    return ds.Tables["Items"];
                }
            }
        }

        // Remove a user by ID
        public int RemoveUser(int userId)
        {
            string sqlQuery = "DELETE FROM Users WHERE id = @Id";

            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Id", userId);
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        // Retrieve all purchases for a specific user by username
        public List<Purchase> GetPurchasesByUsername(string username)
        {
            string sqlQuery = @"
        SELECT p.id, p.purchase_date, u.first_name, u.last_name, u.email, u.phone_number, u.address, u.sex,
        ip.item_id, ip.quantity, ip.item_price_at_purchase, i.name as item_name
        FROM Purchases p
        JOIN Users u ON p.user_id = u.id
        JOIN ItemPurchases ip ON p.id = ip.purchase_id
        JOIN Items i ON ip.item_id = i.Id
        WHERE u.username = @Username";

            var purchases = new List<Purchase>();

            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var purchase = purchases.FirstOrDefault(p => p.Id == reader.GetInt32("id"));
                            if (purchase == null)
                            {
                                purchase = new Purchase
                                {
                                    Id = reader.GetInt32("id"),
                                    PurchaseDate = reader.GetDateTime("purchase_date"),
                                    ItemPurchases = new List<ItemPurchase>()
                                };
                                purchases.Add(purchase);
                            }

                            purchase.ItemPurchases.Add(new ItemPurchase
                            {
                                ItemId = reader.GetInt32("item_id"),
                                Quantity = reader.GetInt32("quantity"),
                                ItemPriceAtPurchase = reader.GetDecimal("item_price_at_purchase"),
                                Item = new Item
                                {
                                    name = reader.GetString("item_name")
                                }
                            });
                        }
                    }
                }
            }

            return purchases;
        }

        // Remove an item by ID
        public int RemoveItem(int itemId)
        {
            string sqlQuery = "DELETE FROM Items WHERE id = @Id";

            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Id", itemId);
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        // Update an item's price by ID
        public int UpdateItemPrice(int itemId, decimal newPrice)
        {
            string sqlQuery = "UPDATE Items SET price = @Price WHERE id = @Id";

            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Id", itemId);
                    cmd.Parameters.AddWithValue("@Price", newPrice);
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        // Add this method to your Helper class

        public int UpdateItemStock(int itemId, int newStock)
        {
            string sqlQuery = "UPDATE Items SET stock = @stock WHERE id = @id";

            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Id", itemId);
                    cmd.Parameters.AddWithValue("@stock", newStock);
                    return cmd.ExecuteNonQuery();
                }
            }
        }


    }
}
