using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetFragrant_Test.Models
{
    public class Discount
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string DiscoutID { get; set; }
        public string DiscoutName { get; set; }
        public string Description { get; set; }
        public decimal DiscountValue { get; set; }
        public string DiscountType { get; set; }
        public DateTime Period { get; set; }

    }
}
