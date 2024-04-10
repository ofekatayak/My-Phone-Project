using System;
namespace store.Models
{
    public class Category
    {
        
        public string CategoryName { get; set; }
        public string ImageUrl { get; set; }

        public Category()  // Default constructor with default values
        {
            CategoryName = string.Empty;
            ImageUrl = string.Empty;
        }

        public Category(string name, string imageUrl)
        {
            CategoryName = name;
            ImageUrl = imageUrl;
        }
    }
}

