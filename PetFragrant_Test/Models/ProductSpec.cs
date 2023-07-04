using System.ComponentModel.DataAnnotations;

namespace PetFragrant_Test.Models
{
    public class ProductSpec
    {

        [Display(Name = "規格ID")]
        public string SpecID { get; set; }
        [Display(Name = "產品ID")]
        public string ProdcutId { get; set; }

        public Spec Spec { get; set; }
        public Product Product { get; set; }    
    }
}
