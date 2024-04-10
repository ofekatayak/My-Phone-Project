using System;
namespace store.Models
{
    public class Product
    {
        public string ProductID { get; set; }
        public string CategoryName { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public byte[] ImageData { get; set; } // New property to store image data
        public bool IsPopular { get; set; }
        public DateTime UploadDate { get; set; }
        public int Sale { get; set; }

        public Product()
        {
            // Assign default values to each property
            ProductID = string.Empty; // Empty string for identifier
            CategoryName = string.Empty;
            Name = string.Empty;
            Price = 0; // Decimal zero for price
            Quantity = 0; // Zero for quantity
            ImageUrl = string.Empty;
            IsPopular = false;
            UploadDate = DateTime.Now;
            Sale = 0;
        }

        public Product(string productId, string categoryName, string name, int price, int quantity, string imageUrl, byte[] imageData, bool isPopular, DateTime uploadDate, int sale = 0)
        {
            ProductID = productId;
            CategoryName = categoryName;
            Name = name;
            Price = price;
            Quantity = quantity;
            ImageUrl = imageUrl;
            ImageData = imageData;
            IsPopular = isPopular;
            UploadDate = uploadDate;
            Sale = 0;
        }
    }

}

