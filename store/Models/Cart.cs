using System;
namespace store.Models
{
	public class Cart
	{
        
        public  List<CartItem> CartItems { get; set; }
		public decimal TotalPrice { get; set; }

        public Cart()
		{
            CartItems = new List<CartItem>();
			TotalPrice = 0;
        }

        public Cart( List<CartItem> items,decimal totalPrice)
		{
	
			CartItems = items;
			TotalPrice = totalPrice;
		}

    }
}

