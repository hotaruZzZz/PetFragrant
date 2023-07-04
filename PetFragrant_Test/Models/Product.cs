using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetFragrant_Test.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "產品ID")]
        public string ProdcutId { get; set; }
        [Display(Name = "類別ID")]
        public string CategoriesID { get; set; }
        [Display(Name = "產品名稱")]
        public string ProductName { get; set; }
        [Display(Name = "產品描述")]
        public string ProductDescription { get; set; }
        [Display(Name = "產品單價")]
        public decimal Price { get; set; }
        [Display(Name = "庫存")]
        public int Inventory { get; set; }
        
        public ICollection<OrderDetail> OrderDetails { get; set; }
        public ICollection<ProductSpec> ProductSpecs { get; set; }
        public ICollection<MyLike> MyLike { get; set; }
        public ICollection<ShoppingCart> ShoppingCarts { get; set;}



        [ForeignKey("CategoriesID")]
        public Categories Categories { get; set; }
    }
}
