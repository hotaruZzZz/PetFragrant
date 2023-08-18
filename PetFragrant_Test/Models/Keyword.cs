using System.ComponentModel.DataAnnotations;

namespace PetFragrant_Test.Models
{
    public class Keyword
    {
        [Key]
        public string Name { get; set; }
        public int Amount { get; set; } 
    }
}
