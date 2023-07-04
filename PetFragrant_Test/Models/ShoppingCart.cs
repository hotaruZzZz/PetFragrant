using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace PetFragrant_Test.Models
{
    public class ShoppingCart
    {
        [Display(Name = "產品ID")]
        public string ProdcutId { get; set; }
        [Display(Name = "顧客ID")]
        public string CustomerID { get; set; }
        [Display(Name = "購買數量")]
        public int Quantity { get; set; }
        [Display(Name = "規格")]
        public string SpecID { get; set; }

        public Product Product { get; set; }
        public Customer Customer { get; set; }

    }
}
