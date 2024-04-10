using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;

using Newtonsoft.Json;
using store.Models;

namespace store.Services
{
	public class OrderService
	{
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserService _userService;
        private readonly CartService _cartService;

       public OrderService(IHttpContextAccessor httpContextAccessor,UserService userService, CartService cartService)
		{
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            _cartService = cartService;
        }

        public string GenerateOrderId()
        {
            return Guid.NewGuid().ToString();
        }

        public async Task PlaceOrder(string email)
        {
            // fetch the Cart
            Cart cart = _cartService.GetCartFromSession();

            string emailSession = _httpContextAccessor.HttpContext.Session.GetString("email") ?? email;
            // create Order in table Order(orderId,UserEmail,Status,CreatedAt,TotalPay)

            Order order = new Order
            {
                OrderID = GenerateOrderId(),
                UserEmail = emailSession,
                Status = "Pending",
                CreatedAt = DateTime.Now,
                TotalPay = cart.TotalPrice,
                
            };

            await _userService.AddOrderToDb(order);

            // of evrey item in Cart Create OrderItem in OrderItem table
            foreach(CartItem item in cart.CartItems)
            {
                // create orderItem 
                OrderItem orderItem = new OrderItem
                {
                    OrderItemID = GenerateOrderId(),
                    OrderID = order.OrderID,
                    ProductID = item.Product.ProductID,
                    Quantity =item.Quantity,
                    Product = item.Product
                };

                // insert orderItem to table
                await _userService.AddOrderItemToDb(orderItem);
            }

            // reset the Cart
            _cartService.ResetCart();
        }

    }
}

