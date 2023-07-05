using System;
using System.Collections.Generic;

#nullable disable

namespace PetFragrant_Test.Models
{
    public partial class Coupon
    {
        public string CouponId { get; set; }
        public string OrderId { get; set; }
        public DateTime Period { get; set; }
        public decimal Rate { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }

        public virtual Order Order { get; set; }
    }
}
