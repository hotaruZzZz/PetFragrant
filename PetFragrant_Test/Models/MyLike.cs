using System;
using System.Collections.Generic;

#nullable disable

namespace PetFragrant_Test.Models
{
    public partial class MyLike
    {
        public string ProdcutId { get; set; }
        public string CustomerId { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Product Prodcut { get; set; }
    }
}
