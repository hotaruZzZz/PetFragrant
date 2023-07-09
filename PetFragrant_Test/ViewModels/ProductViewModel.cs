using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PetFragrant_Test.Models;
namespace PetFragrant_Test.ViewModels
{
    public class ProductViewModel
    {
        public Product ProductData { get; set; }
        public Category CategoryData { get; set; }
        public IEnumerable<Spec> SpecData { get; set; }

    }
}
