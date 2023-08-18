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
        // 折扣值
        public decimal DiscountValue { get; set; }
        // 折價類型(折扣 或 百分比)
        public string DiscountType { get; set; }
        // 使用對象
        public string User { get; set; }
        // 起始日期
        public DateTime Start { get; set; }
        // 結束日
        public DateTime Period { get; set; }
        // 最低金額 (滿額折扣)
        public decimal MinimumAmount { get; set; }


    }
}
