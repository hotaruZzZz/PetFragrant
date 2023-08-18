using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace PetFragrant_Test.Models
{
    public partial class Order
    {
        public Order()
        {
            Coupons = new HashSet<Coupon>();
            OrderDetails = new HashSet<OrderDetail>();
        }

        [Key]
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public DateTime Orderdate { get; set; }
        public DateTime Shipdate { get; set; }
        public DateTime Arriiveddate { get; set; }
        public DateTime Paymentdate { get; set; }
        public string Payment { get; set; }
        public string Delivery { get; set; }
        public bool Check { get; set; }
        public string StoreID { get; set; }
        public string CouponID { get; set; }
        public string status { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual ICollection<Coupon> Coupons { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
