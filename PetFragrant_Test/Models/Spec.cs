using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace PetFragrant_Test.Models
{
    public partial class Spec
    {
        public Spec()
        {
            ProductSpecs = new HashSet<ProductSpec>();
            ShoppingCarts = new HashSet<ShoppingCart>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string SpecId { get; set; }
        public string SpecName { get; set; }

        public virtual ICollection<ProductSpec> ProductSpecs { get; set; }
        public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; }
    }
}
