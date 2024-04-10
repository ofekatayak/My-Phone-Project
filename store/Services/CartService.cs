using store.Models;
using store.Services;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace store.Services
{
    public class CartService
    {
        private readonly UserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(UserService userService, IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }

        public Cart GetCartFromSession()
        {
            string jsonCart = _httpContextAccessor.HttpContext.Session.GetString("Cart") ?? string.Empty;
            return JsonConvert.DeserializeObject<Cart>(jsonCart) ?? new Cart();
        }

        private void StoreCartInSession(Cart cart)
        {
            string jsonCart = JsonConvert.SerializeObject(cart);
            _httpContextAccessor.HttpContext.Session.SetString("Cart", jsonCart);
        }

        public async Task RemoveFromCart(string productId)
        {
            // Get the existing cart from session
            var cart = GetCartFromSession();

            // Find the item in the cart
            var existingItem = cart.CartItems.FirstOrDefault(cartItem => cartItem.Product.ProductID == productId);

            if (existingItem != null)
            {
                // Restore product quantity to stock
                for (int i = 0; i < existingItem.Quantity; i++)
                {
                    Product product = await _userService.GetProductById(productId);
                    await _userService.UpdateProductQuantity(productId, product.Quantity + 1);
                    cart.TotalPrice -= product.Price;
                }

                // Remove the entire product from cart
                cart.CartItems.Remove(existingItem);

                // Update the cart in session
                StoreCartInSession(cart);
            }
        }

        public async Task AddToCart(string productId)
        {
            // Get the product
            Product product = await _userService.GetProductById(productId);

            // Update product quantity
            await _userService.UpdateProductQuantity(productId, product.Quantity - 1);

            // Get the existing cart from session
            var cart = GetCartFromSession();

            // Check if item already exists in the cart
            var existingItem = cart.CartItems.FirstOrDefault(cartItem => cartItem.Product.ProductID == productId);

            if (existingItem != null)
            {
                existingItem.Quantity++; // Increment quantity if item exists
                existingItem.Product.Price += product.Price;
            }
            else
            {
                // Add new item if it doesn't exist
                cart.CartItems.Add(new CartItem { Product = product, Quantity = 1 });
            }

            cart.TotalPrice += product.Price;

            // Update the cart in session
            StoreCartInSession(cart);
        }

        public void ResetCart()
        {
            Cart cart = new Cart();
            StoreCartInSession(cart);
        }

        public async Task DecrementCartItem(string productId)
        {
            // Get the product
            Product product = await _userService.GetProductById(productId);

            // Get the existing cart from session
            var cart = GetCartFromSession();

            // Check if item already exists in the cart
            var existingItem = cart.CartItems.FirstOrDefault(cartItem => cartItem.Product.ProductID == productId);

            if (existingItem != null)
            {
                if (existingItem.Quantity > 1)
                {
                    // Update product quantity
                    await _userService.UpdateProductQuantity(productId, product.Quantity + 1);
                    existingItem.Quantity--; // Increment quantity if item exists
                    existingItem.Product.Price -= product.Price;
                    cart.TotalPrice -= product.Price;
                    // Update the cart in session
                    StoreCartInSession(cart);
                }

                else
                {
                    await RemoveFromCart(productId);
                }

            }

        }
    }
}
