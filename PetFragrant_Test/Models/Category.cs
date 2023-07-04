using System;
using System.Collections.Generic;

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

        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string FatherCategoryId { get; set; }

        public virtual Category FatherCategory { get; set; }
        public virtual ICollection<Category> InverseFatherCategory { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
