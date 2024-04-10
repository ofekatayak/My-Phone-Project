using System;

namespace store.Models
{
    public class Order
    {
        public string OrderID { get; set; }
        public string UserEmail{ get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalPay { get; set; }
        public List<OrderItem> OrderItems { get; set; }

        public Order()
        {
            OrderID = string.Empty;
            UserEmail = string.Empty;
            Status = string.Empty;
            CreatedAt = DateTime.Now;
            TotalPay = 0.0m;
            OrderItems = new List<OrderItem>();
        }

        public Order(string orderId, string userEmail,  DateTime createdAt,decimal totalPrice, List<OrderItem> orderItems)
        {
            OrderID = orderId;
            UserEmail = userEmail;
            Status = "Panding";
            CreatedAt = createdAt;
            TotalPay = totalPrice;
            OrderItems = orderItems;
        }
    }
}