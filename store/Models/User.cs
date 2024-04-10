using System;

namespace store.Models
{
    public class User
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }

        public User() { }

        public User( string username, string email, string password, bool isAdmin = false)
        {
            Name = username;
            Email = email;
            Password = password;
            IsAdmin = isAdmin; // Default value for isAdmin
        }
    }


}
