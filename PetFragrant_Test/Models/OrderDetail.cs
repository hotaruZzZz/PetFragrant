using System;
using System.Collections.Generic;

#nullable disable

namespace PetFragrant_Test.Models
{
    public partial class OrderDetail
    {
        public string ProdcutId { get; set; }
        public string OrderId { get; set; }
        public int Amount { get; set; }
        public decimal QtDiscount { get; set; }

        public virtual Order Order { get; set; }
        public virtual Product Prodcut { get; set; }
    }
}
