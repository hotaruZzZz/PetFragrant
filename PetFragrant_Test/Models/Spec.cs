using System;
using System.Collections.Generic;

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

        public string SpecId { get; set; }
        public string SpecName { get; set; }

        public virtual ICollection<ProductSpec> ProductSpecs { get; set; }
        public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; }
    }
}
