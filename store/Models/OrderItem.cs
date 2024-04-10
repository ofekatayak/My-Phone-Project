using System;
using Microsoft.CodeAnalysis;
using Mysqlx.Crud;

namespace store.Models
{
    public class OrderItem
    {
        public string OrderItemID;
        public string OrderID { get; set; }
        public string ProductID { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }


        public OrderItem() // Default constructor with default values
        {
            OrderItemID = "";
            OrderID = string.Empty;
            ProductID = string.Empty;
            Quantity = 0;
            Product = new Product();
        }

        public OrderItem(string otderItemId,string orderId, string productId, int quantity,Product product)
        {
            OrderItemID = otderItemId;
            OrderID = orderId;
            ProductID = productId;
            Quantity = quantity;
            Product = product;
        }
    }
}