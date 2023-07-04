using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace PetFragrant_Test.Models
{
    public class Spec
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "規格ID")]
        public string SpecID { get; set; }
        [Display(Name = "規格名稱")]
        public string SpecName { get; set; }
        public ICollection<ProductSpec> ProductSpec { get; set; }
    }
}
