using System;
using System.Collections.Generic;

#nullable disable

namespace PetFragrant_Test.Models
{
    public partial class Customer
    {
        public Customer()
        {
            MyLikes = new HashSet<MyLike>();
            Orders = new HashSet<Order>();
            ShoppingCarts = new HashSet<ShoppingCart>();
        }

        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public bool IsAdmin { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Level { get; set; }
        public DateTime Birthday { get; set; }
        public bool? EmailConfirmed { get; set; }

        public virtual ICollection<MyLike> MyLikes { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; }
    }
}
