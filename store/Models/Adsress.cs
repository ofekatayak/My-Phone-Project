using System;
namespace store.Models
{
    public class Address
    {
        public string UserEmail { get; set; }
        public string UserAddress { get; set; }
        public string City { get; set; }
        public int ZipCode { get; set; }

        public Address(string email, string userAddress, string city, int zip)
        {
            UserEmail = email;
            UserAddress = userAddress;
            City = city;
            ZipCode = zip;
        }
    }

}

