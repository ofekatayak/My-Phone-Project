
using store.Models;

namespace store.ViewModels
{
    public class HomeViewModel
    {
        public User User { get; set; }
        public List<Product> Products { get; set; }
        public List<Category> Categories { get; set; }
        public Cart Cart { get; set; }
        public Notification Notification { get; set; }
        public bool IsPayVisible { get; set; }
        public bool isCartVisible { get; set; }
    }
}
