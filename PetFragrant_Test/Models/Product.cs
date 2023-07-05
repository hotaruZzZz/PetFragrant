using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name ="產品ID")]
        public string ProdcutId { get; set; }
        [Display(Name = "類別ID")]
        public string CategoriesId { get; set; }
        [Display(Name = "產品名稱")]
        public string ProductName { get; set; }
        [Display(Name = "產品描述")]
        public string ProductDescription { get; set; }
        [Display(Name = "產品單價")]
        public decimal Price { get; set; }
        [Display(Name = "產品庫存")]
        public int Inventory { get; set; }

        public virtual Category Categories { get; set; }
        public virtual ICollection<MyLike> MyLikes { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<ProductSpec> ProductSpecs { get; set; }
        public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; }
    }
}
