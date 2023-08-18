using PetFragrant_Test.Models;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PetFragrant_Test.ViewModels
{
    public class OrderDetailViewModel
    {
        public string OrderId { get; set; }
        public DateTime Orderdate { get; set; }
        public DateTime Shipdate { get; set; }
        public DateTime Arriiveddate { get; set; }
        public DateTime PaymentDate { get; set; }
        public bool Check { get; set; }
        public string Status { get; set; }
        public string Payment { get; set; }
        public string Delivery { get; set; }
        public decimal TotalPrice { get; set; }
        public string CouponName { get; set; }
        public string StoreName { get; set; }

        public IEnumerable<OrderProductViewModel> orderproduct { get; set; }
    }
}
