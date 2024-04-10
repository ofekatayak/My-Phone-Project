using System.Threading.Tasks;
using MySqlConnector;
using Microsoft.Extensions.Configuration;
using store.Models;
using Microsoft.CodeAnalysis;
using Microsoft.Build.Evaluation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Diagnostics.Metrics;

namespace store.Services
{
    public class UserService
    {
        private readonly string _connectionString;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public UserService(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            _connectionString = configuration.GetConnectionString("Default");
            _hostingEnvironment = hostingEnvironment;
        }

        //----------User Functions----------//
        public async Task AddUser(User newUser)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            string query = "INSERT INTO Users (Name, Email, Password, IsAdmin) VALUES (@Name, @Email, @Password, @IsAdmin)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Name", newUser.Name);
            command.Parameters.AddWithValue("@Email", newUser.Email);
            command.Parameters.AddWithValue("@Password", newUser.Password);
            command.Parameters.AddWithValue("@IsAdmin", newUser.IsAdmin);
            await command.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }

        public async Task<bool> CheckEmailExists(string email)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            string query = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Email", email);
            var emailExists = (long)await command.ExecuteScalarAsync() > 0;
            await connection.CloseAsync();
            return emailExists;
        }

        public async Task<bool> ValidateUser(string email, string password)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            string query = "SELECT COUNT(*) FROM Users WHERE Email = @Email AND Password = @Password";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Email", email);
            command.Parameters.AddWithValue("@Password", password);
            var userCount = (long)await command.ExecuteScalarAsync();
            await connection.CloseAsync();
            return userCount > 0;
        }

        public async Task<List<Product>> GetProductsFromDatabase()
        {
            var products = new List<Product>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = "SELECT * FROM Product";
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var product = new Product
                {
                    ProductID = reader.GetString("ProductID"),
                    CategoryName = reader.GetString("CategoryName"),
                    Name = reader.GetString("Name"),
                    Price = reader.GetInt32("Price"),
                    Quantity = reader.GetInt32("Quantity"),
                    ImageUrl = reader.GetString("ImageUrl"),
                    IsPopular = reader.GetBoolean("IsPopular"),
                    Sale = reader.GetInt32("SalePrice")
                };
                // Extracting image data as a byte array
                var imageData = (byte[])reader["ImageData"];
                product.ImageData = imageData;

                // Extracting only the date part from the DateTime value
                var uploadDate = reader.GetDateTime("UploadDate").Date;

                // Assign the date to the product
                product.UploadDate = uploadDate;

                products.Add(product);
            }

            foreach (var product in products)
            {
                // Convert byte array to Base64 string
                product.ImageUrl = $"data:image/jpeg;base64,{Convert.ToBase64String(product.ImageData)}";
            }
            await connection.CloseAsync();
            return products;
        }

        public async Task<List<Category>> GetCategoriesFromDatabase()
        {
            var categories = new List<Category>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = "SELECT CategoryName, ImageUrl FROM Category";
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var category = new Category
                {
                    CategoryName = reader.GetString("CategoryName"),
                    ImageUrl = reader.GetString("ImageUrl")
                };
                categories.Add(category);
            }
            return categories;
        }

        public async Task<List<Order>> GetOrdersWithProductDetails(string userEmail)
        {
            var ordersWithDetails = new List<Order>();
            string query = @"
            SELECT 
                O.OrderID,
                O.UserEmail,
                O.Status,
                O.CreatedAt,
                O.TotalPay,
                OI.OrderItemID,
                OI.ProductID,
                OI.Quantity,
                P.CategoryName,
                P.Name AS ProductName,
                P.Price,
                P.ImageUrl,
                P.IsPopular,
                P.UploadDate
            FROM 
                `Order` O
            INNER JOIN 
                OrderItem OI ON O.OrderID = OI.OrderID
            INNER JOIN 
                Product P ON OI.ProductID = P.ProductID
            WHERE 
                O.UserEmail = @UserEmail";

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserEmail", userEmail);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var orderId = reader.GetString("OrderID");

                            var existingOrder = ordersWithDetails.Find(order => order.OrderID == orderId);

                            if (existingOrder == null)
                            {
                                existingOrder = new Order
                                {
                                    OrderID = orderId,
                                    UserEmail = userEmail,
                                    Status = reader.GetString("Status"),
                                    CreatedAt = reader.GetDateTime("CreatedAt"),
                                    TotalPay = reader.GetDecimal("TotalPay"),
                                    OrderItems = new List<OrderItem>()
                                };

                                ordersWithDetails.Add(existingOrder);
                            }

                            var orderItem = new OrderItem
                            {
                                OrderItemID = reader.GetString("OrderItemID"),
                                OrderID = orderId,
                                ProductID = reader.GetString("ProductID"),
                                Product = new Product
                                {
                                    ProductID = reader.GetString("ProductID"),
                                    CategoryName = reader.GetString("CategoryName"),
                                    Name = reader.GetString("ProductName"),
                                    Price = reader.GetInt32("Price"),
                                    ImageUrl = reader.GetString("ImageUrl"),
                                    IsPopular = reader.GetBoolean("IsPopular"),
                                    UploadDate = reader.GetDateTime("UploadDate")
                                },
                                Quantity = reader.GetInt32("Quantity"),
                            };

                            existingOrder.OrderItems.Add(orderItem);
                        }

                    }
                }
            }

            return ordersWithDetails;
        }

        public async Task<User> GetUserFromDatabase(string email)
        {
            User user = null;
            string query = "SELECT * FROM Users WHERE Email = @Email";
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            user = new User
                            {
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Password = reader.GetString(reader.GetOrdinal("Password")),
                                IsAdmin = reader.GetBoolean(reader.GetOrdinal("IsAdmin"))
                            };
                        }
                    }
                }
            }
            return user;
        }

        public async Task<User> Login(string email, string password)
        {
            User user = null;
            string query = "SELECT * FROM Users WHERE Email = @Email AND Password = @Password";
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Password", password);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            user = new User
                            {
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Password = reader.GetString(reader.GetOrdinal("Password")),
                                IsAdmin = reader.GetBoolean(reader.GetOrdinal("IsAdmin"))
                            };
                        }
                    }
                }
                await connection.CloseAsync();
            }

            return user;
        }

        //----------Cart----------//
        public async Task<Product> GetProductById(string productId)
        {
            Product? product = null;
            string query = "SELECT * FROM Product WHERE ProductID = @ProductID";
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductID", productId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            product = new Product
                            {
                                ProductID = reader.GetString("ProductID"),
                                CategoryName = reader.GetString("CategoryName"),
                                Name = reader.GetString("Name"),
                                Price = reader.GetInt32("Price"),
                                Quantity = reader.GetInt32("Quantity"),
                                ImageUrl = reader.GetString("ImageUrl"),
                                IsPopular = reader.GetBoolean("IsPopular"),
                                UploadDate = reader.GetDateTime("UploadDate").Date,
                                Sale = reader.GetInt32("SalePrice")
                            };

                            // Extracting image data as a byte array
                            var imageData = (byte[])reader["ImageData"];
                            product.ImageData = imageData;
                        }


                    }
                }
                product.ImageUrl = $"data:image/jpeg;base64,{Convert.ToBase64String(product.ImageData)}";
                await connection.CloseAsync();
            }
            return product;
        }

        public async Task UpdateProductQuantity(string productId, int newQuantity)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // Construct the SQL command to update the product quantity
            var sql = "UPDATE Product SET Quantity = @NewQuantity WHERE ProductID = @ProductId";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@NewQuantity", newQuantity);
            command.Parameters.AddWithValue("@ProductId", productId);

            // Execute the command
            await command.ExecuteNonQueryAsync();
        }

        //----------Search----------//
        public async Task<List<Product>> SearchProducts(string name)
        {
            var products = new List<Product>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = "SELECT * FROM Product WHERE Name LIKE @name";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@name", "%" + name + "%");

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var product = new Product
                            {
                                ProductID = reader.GetString("ProductID"),
                                CategoryName = reader.GetString("CategoryName"),
                                Name = reader.GetString("Name"),
                                Price = reader.GetInt32("Price"),
                                Quantity = reader.GetInt32("Quantity"),
                                ImageUrl = reader.GetString("ImageUrl"),
                                IsPopular = reader.GetBoolean("IsPopular"),
                                UploadDate = reader.GetDateTime("UploadDate").Date,
                                Sale = reader.GetInt32("SalePrice")
                            };
                            var imageData = (byte[])reader["ImageData"];
                            product.ImageData = imageData;
                            product.ImageUrl = $"data:image/jpeg;base64,{Convert.ToBase64String(product.ImageData)}";
                            products.Add(product);
                        }
                    }
                }
            }

            return products;
        }

        //----------***----------//
        public List<Product> GetFilteredProducts(string query)
        {
            List<Product> products = new List<Product>();
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    // Construct Product object from reader data
                    Product product = new Product
                    {
                        ProductID = reader.GetString("ProductID"),
                        CategoryName = reader.GetString("CategoryName"),
                        Name = reader.GetString("Name"),
                        Price = reader.GetInt32("Price"),
                        Quantity = reader.GetInt32("Quantity"),
                        ImageUrl = reader.GetString("ImageUrl"),
                        IsPopular = reader.GetBoolean("IsPopular"),
                        UploadDate = reader.GetDateTime("UploadDate").Date,
                        Sale = reader.GetInt32("SalePrice")

                    };
                    // Extracting image data as a byte array
                    var imageData = (byte[])reader["ImageData"];
                    product.ImageData = imageData;
                    product.ImageUrl = $"data:image/jpeg;base64,{Convert.ToBase64String(product.ImageData)}";
                    products.Add(product);
                }       
                connection.Close();
            }
            return products;
        }

        public async Task AddOrderToDb(Order order)
        {
                // Create a MySqlConnection using your connection string
                using (var connection = new MySqlConnection(_connectionString))
                {
                    // Open the database connection
                    await connection.OpenAsync();
                    string formattedCreatedAt = order.CreatedAt.ToString("yyyy/MM/dd");
                    // Construct the SQL query
                    string query = @"INSERT INTO `Order` (OrderID, UserEmail, Status, CreatedAt, TotalPay) VALUES (@OrderID, @UserEmail, @Status, @CreatedAt, @TotalPay)";

                    // Create a MySqlCommand with the query and connection
                    using (var command = new MySqlCommand(query, connection))
                    {
                        // Add parameters to the query
                        command.Parameters.AddWithValue("@OrderID", order.OrderID);
                        command.Parameters.AddWithValue("@UserEmail", order.UserEmail);
                        command.Parameters.AddWithValue("@Status", order.Status);
                        command.Parameters.AddWithValue("@CreatedAt", formattedCreatedAt);
                        command.Parameters.AddWithValue("@TotalPay", order.TotalPay);

                        // Execute the query
                        await command.ExecuteNonQueryAsync();
                    }
                }
            
        }

        public async Task AddOrderItemToDb(OrderItem orderItem)
        {
            try
            {
                // Create a MySqlConnection using your connection string
                using (var connection = new MySqlConnection(_connectionString))
                {
                    // Open the database connection
                    await connection.OpenAsync();


                    // Construct the SQL query
                    string query = @"INSERT INTO `OrderItem` (OrderItemID ,OrderID, ProductID, Quantity) 
                             VALUES (@OrderItemID,@OrderID, @ProductID, @Quantity)";

                    // Create a MySqlCommand with the query and connection
                    using (var command = new MySqlCommand(query, connection))
                    {
                        // Add parameters to the query
                        command.Parameters.AddWithValue("@OrderItemID", orderItem.OrderItemID);
                        command.Parameters.AddWithValue("@OrderID", orderItem.OrderID);
                        command.Parameters.AddWithValue("@ProductID", orderItem.Product.ProductID);
                        command.Parameters.AddWithValue("@Quantity", orderItem.Quantity);

                        // Execute the query
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                Console.WriteLine("Error inserting order item: " + ex.Message);
            }
        }

        public async Task AddCreditCardDetailsToDB(string userEmail, string cardNumber, string expiryDate, string cvvCode)
        {
            AesManaged aes = new AesManaged();
            // Generate a random key and IV (Initialization Vector)
            aes.GenerateKey();
            aes.GenerateIV();

            // Convert the credit card number to bytes
            byte[] cardNumberBytes = System.Text.Encoding.UTF8.GetBytes(cardNumber);

            // Encrypt the credit card number using AES encryption
            byte[] encryptedCardNumberBytes = EncryptBytesWithAES(cardNumberBytes, aes.Key, aes.IV);

            // Convert the encrypted bytes to a base64-encoded string for storage
            string encryptedCardNumber = Convert.ToBase64String(encryptedCardNumberBytes);

            CreditCard newCreditCard = new CreditCard(userEmail,encryptedCardNumber, expiryDate, cvvCode);

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            string query = "INSERT INTO CreditCard (UserEmail, CardNumber, CardDate, CVV) VALUES (@UserEmail, @CardNumber, @CardDate, @CVV)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserEmail", newCreditCard.UserEmail);
            command.Parameters.AddWithValue("@CardNumber", newCreditCard.CardNumber);
            command.Parameters.AddWithValue("@CardDate", newCreditCard.CardDate);
            command.Parameters.AddWithValue("@CVV", newCreditCard.CardCvv);
            await command.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }

        private byte[] EncryptBytesWithAES(byte[] dataToEncrypt, byte[] key, byte[] IV)
        {
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        // Write all data to the crypto stream and flush it
                        csEncrypt.Write(dataToEncrypt, 0, dataToEncrypt.Length);
                        csEncrypt.FlushFinalBlock();

                        // Return the encrypted bytes from the memory stream
                        return msEncrypt.ToArray();
                    }
                }
            }
        }

        public async Task AddAddressToDB(string email, string userAddress, string userCity, int userZipCode)
        {
            Address newAdress = new Address(email, userAddress, userCity, userZipCode);

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            string query = "INSERT INTO Address (UserEmail, Street, City, ZipCode) VALUES (@UserEmail, @Street, @City, @ZipCode)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserEmail", newAdress.UserEmail);
            command.Parameters.AddWithValue("@Street", newAdress.UserAddress);
            command.Parameters.AddWithValue("@City", newAdress.City);
            command.Parameters.AddWithValue("@ZipCode", newAdress.ZipCode);
            await command.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }

        //Admin Functions
        public async Task AddProduct(Product product)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // Fetch the maximum product ID from the database
            var maxProductIdQuery = "SELECT MAX(CAST(SUBSTRING(ProductID, 2) AS UNSIGNED)) FROM Product";
            using var maxProductIdCommand = new MySqlCommand(maxProductIdQuery, connection);
            var maxProductId = await maxProductIdCommand.ExecuteScalarAsync();
            var newProductIdNumber = (maxProductId == null || maxProductId == DBNull.Value) ? 1 : Convert.ToUInt64(maxProductId) + 1;

            // Construct the new ProductID
            var newProductId = "p" + newProductIdNumber;

            // Construct the SQL command to insert the product into the database
            var sql = @"INSERT INTO Product (ProductID, CategoryName, Name, Price, Quantity, ImageUrl, ImageData, IsPopular, UploadDate) 
                VALUES (@ProductID, @CategoryName, @Name, @Price, @Quantity, @ImageUrl, @ImageData, @IsPopular, @UploadDate)";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@ProductID", newProductId);
            command.Parameters.AddWithValue("@CategoryName", product.CategoryName);
            command.Parameters.AddWithValue("@Name", product.Name);
            command.Parameters.AddWithValue("@Price", product.Price);
            command.Parameters.AddWithValue("@Quantity", product.Quantity);
            command.Parameters.AddWithValue("@ImageUrl", product.ImageUrl);
            command.Parameters.AddWithValue("@ImageData", product.ImageData); // Assuming ImageData is a byte[]
            command.Parameters.AddWithValue("@IsPopular", product.IsPopular);
            command.Parameters.AddWithValue("@UploadDate", product.UploadDate);

            // Execute the command
            await command.ExecuteNonQueryAsync();

            // Close the connection
            await connection.CloseAsync();
        }

        public async Task<byte[]> SaveImage(IFormFile image)
        {
            // Read the image data into a byte array
            byte[] imageData;
            using (MemoryStream stream = new MemoryStream())
            {
                await image.CopyToAsync(stream);
                imageData = stream.ToArray();
            }

            // Return the image data
            return imageData;
        }

        public async Task DeleteProduct(string productId)
        {
            // Implement logic to delete the product from the database based on the productId
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = "DELETE FROM Product WHERE ProductID = @ProductId";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);
                    await command.ExecuteNonQueryAsync();
                }
                await connection.CloseAsync();
            }
        }

        public async Task UpdateProduct(Product updatedProduct)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // Construct the SQL command to update the product in the database
            var sql = @"UPDATE Product 
                SET CategoryName = @CategoryName, 
                    Name = @Name, 
                    Price = @Price, 
                    Quantity = @Quantity, 
                    ImageUrl = @ImageUrl, 
                    ImageData = @ImageData, 
                    IsPopular = @IsPopular, 
                    UploadDate = @UploadDate,
                    SalePrice = @SalePrice
                WHERE ProductID = @ProductId";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@CategoryName", updatedProduct.CategoryName);
            command.Parameters.AddWithValue("@Name", updatedProduct.Name);
            command.Parameters.AddWithValue("@Price", updatedProduct.Price);
            command.Parameters.AddWithValue("@Quantity", updatedProduct.Quantity);
            command.Parameters.AddWithValue("@ImageUrl", updatedProduct.ImageUrl);
            command.Parameters.AddWithValue("@ImageData", updatedProduct.ImageData);
            command.Parameters.AddWithValue("@IsPopular", updatedProduct.IsPopular);
            command.Parameters.AddWithValue("@UploadDate", updatedProduct.UploadDate);
            command.Parameters.AddWithValue("@ProductId", updatedProduct.ProductID);
            command.Parameters.AddWithValue("@SalePrice", updatedProduct.Sale);
            // Execute the command
            await command.ExecuteNonQueryAsync();

            // Close the connection
            await connection.CloseAsync();
        }

        public async Task OrderOneProduct(string productId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // Construct the SQL command to update the product quantity
            var sql = "UPDATE Product SET Quantity = Quantity + 1 WHERE ProductID = @ProductId";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@ProductId", productId);
            // Execute the command
            await command.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }

        //Notification Functions
        public async Task Notify(string productID, string UserEmail)
        {
            // notify button action
            /* insert into notification table the details of the product and the user who wants it */
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            string query = "INSERT INTO Notification (ProductID,UserEmail) VALUES (@ProductId, @UserEmail)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@ProductID", productID);
            command.Parameters.AddWithValue("@UserEmail", UserEmail);
            await command.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }

        public async Task<bool> NotifyKeyCheck(string productID, string UserEmail)
        {
            /* checks if the user is already notify the same product more than once */
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            string query = "SELECT COUNT(*) FROM Notification WHERE ProductID = @ProductID AND UserEmail = @UserEmail";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@ProductID", productID);
            command.Parameters.AddWithValue("@UserEmail", UserEmail);
            var answer = (long)await command.ExecuteScalarAsync() > 0;
            await connection.CloseAsync();
            return answer;
        }

        public async Task<Notification> getUserNotification(string UserEmail)
        {
            Notify? note = null;
            Product? product = null;
            Notification notifies = new Notification();
            string query = "SELECT DISTINCT * FROM Notification n1,Product p1" +
                " WHERE n1.ProductID = p1.ProductID AND n1.UserEmail = @UserEmail";
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserEmail", UserEmail);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            note = new Notify
                            {
                                product = new Product
                                {
                                    ProductID = reader.GetString("ProductID"),
                                    CategoryName = reader.GetString("CategoryName"),
                                    Name = reader.GetString("Name"),
                                    Price = reader.GetInt32("Price"),
                                    Quantity = reader.GetInt32("Quantity"),
                                    ImageUrl = reader.GetString("ImageUrl"),
                                    IsPopular = reader.GetBoolean("IsPopular"),
                                    UploadDate = reader.GetDateTime("UploadDate").Date,
                                }, Email = UserEmail,
                            };
                            
                            notifies.List_Notification.Add(note);
                        }
                    }
                }
                await connection.CloseAsync();
            }
            return notifies;
        }

        public async Task RemoveNotify(string productID, string UserEmail)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            string query = "DELETE FROM Notification WHERE ProductID = @ProductID AND UserEmail = @UserEmail";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@ProductID", productID);
            command.Parameters.AddWithValue("@UserEmail", UserEmail);
            await command.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }
    }
}