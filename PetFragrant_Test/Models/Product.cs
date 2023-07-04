using System;
using System.Collections.Generic;

#nullable disable

namespace PetFragrant_Test.Models
{
    public partial class Product
    {
        public Product()
        {
            MyLikes = new HashSet<MyLike>();
            OrderDetails = new HashSet<OrderDetail>();
            ProductSpecs = new HashSet<ProductSpec>();
            ShoppingCarts = new HashSet<ShoppingCart>();
        }

        public string ProdcutId { get; set; }
        public string CategoriesId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal Price { get; set; }
        public int Inventory { get; set; }

        public virtual Category Categories { get; set; }
        public virtual ICollection<MyLike> MyLikes { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<ProductSpec> ProductSpecs { get; set; }
        public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; }
    }
}
