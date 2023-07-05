using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace PetFragrant_Test.Models
{
    public partial class Category
    {
        public Category()
        {
            InverseFatherCategory = new HashSet<Category>();
            Products = new HashSet<Product>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string FatherCategoryId { get; set; }

        public virtual Category FatherCategory { get; set; }
        public virtual ICollection<Category> InverseFatherCategory { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
