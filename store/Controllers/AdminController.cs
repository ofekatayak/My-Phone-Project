using Microsoft.AspNetCore.Mvc;
using store.Models;
using store.Services;

namespace MyPhone.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserService _userService;

        public AdminController(UserService userService)
        {
            _userService = userService;
        }

        public IActionResult ProductsView()
        {
            return View();
        }

        public async Task<IActionResult> GetProducts()
        {
            var products = await _userService.GetProductsFromDatabase();

            return View("ProductsView", products);
        }

        public async Task<IActionResult> GetCategories()
        {
            var categories = await _userService.GetCategoriesFromDatabase();
            return Json(categories);
        }

        public async Task<IActionResult> OrderOneProduct(string productID)
        {
            await _userService.OrderOneProduct(productID);
            return RedirectToAction("GetProducts");
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(string productName, string price, string category, string quantity, string isPopular, IFormFile image)
        {
            // Convert price and quantity to integer
            int.TryParse(price, out int parsedPrice);
            int.TryParse(quantity, out int parsedQuantity);

            // Convert isPopular to boolean
            bool parsedIsPopular = isPopular.ToLower() == "yes";

            // Save the image and get its URL
            byte[] imageData = await _userService.SaveImage(image);

            // Create a new product object
            var product = new Product
            {
                Name = productName,
                Price = parsedPrice,
                CategoryName = category,
                Quantity = parsedQuantity,
                IsPopular = parsedIsPopular,
                ImageUrl = "",
                ImageData = imageData, // Assign the image data
                UploadDate = DateTime.Now
            };

            // Add the product to the database
            await _userService.AddProduct(product);

            // Redirect to the product view page
            return RedirectToAction("GetProducts", "Admin");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(string productId)
        {
            // Call the UserService to delete the product
            await _userService.DeleteProduct(productId);
            // Return a success message
            return RedirectToAction("GetProducts");
        }

        [HttpGet]
        public async Task<IActionResult> GetProductDetails(string productId)
        {
            var product = await _userService.GetProductById(productId); // Implement this method to fetch product details from the database

            var responseObject = new
            {
                name = product.Name,
                price = product.Price,
                category = product.CategoryName,
                quantity = product.Quantity,
                isPopular = product.IsPopular ? "yes" : "no", // Convert boolean to string representation
                imageUrl = product.ImageUrl, // Include the image URL in the response
                sale = product.Sale
            };

            return Json(responseObject);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProduct(string productId, string productName, string price, string category, string quantity, string isPopular, IFormFile image, string sale)
        {
            // Convert price and quantity to integer
            int.TryParse(price, out int parsedPrice);
            int.TryParse(quantity, out int parsedQuantity);
            int.TryParse(sale, out int parsedSale);
            // Convert isPopular to boolean
            bool parsedIsPopular = isPopular.ToLower() == "yes";

            // Fetch the product from the database
            var existingProduct = await _userService.GetProductById(productId);

            // Update the product properties
            existingProduct.Name = productName;
            existingProduct.Price = parsedPrice;
            existingProduct.CategoryName = category;
            existingProduct.Quantity = parsedQuantity;
            existingProduct.IsPopular = parsedIsPopular;
            existingProduct.Sale = parsedSale;

            // Check if a new image is uploaded
            if (image != null)
            {
                // Save the new image and update the product's image data and URL
                existingProduct.ImageData = await _userService.SaveImage(image);
                existingProduct.ImageUrl = ""; // You may need to update this logic based on how the image URL is generated
            }

            // Update the product in the database
            await _userService.UpdateProduct(existingProduct);

            // Redirect to the product view page
            return RedirectToAction("GetProducts", "Admin");
        }
    }
}
