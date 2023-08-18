using Microsoft.VisualBasic;
using System;

namespace PetFragrant_Test.ViewModels
{
    public class CouponViewModel
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public decimal Value { get; set; }
        public DateTime Period { get; set; }
        public DateTime Start { get; set; }
        public decimal MinimumAmount { get; set; }
        public string User { set; get; }
    }
}
