using System;
using System.Collections.Generic;

#nullable disable

namespace PetFragrant_Test.Models
{
    public partial class ProductSpec
    {
        public string SpecId { get; set; }
        public string ProdcutId { get; set; }

        public virtual Product Product { get; set; }
        public virtual Spec Spec { get; set; }
    }
}
