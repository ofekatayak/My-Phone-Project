using System;
namespace store.Models

{
    public class Notify
    {
        public Product product { get; set; }
        public string Email { get; set; }

        public Notify()
        {
            product = new Product();
            Email = "";
        }

        public Notify(Product product, string email)
        {
            this.product = product;
            this.Email = email;
        }
    }
}

