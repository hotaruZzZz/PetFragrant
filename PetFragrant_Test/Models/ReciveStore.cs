using System.ComponentModel.DataAnnotations;

namespace PetFragrant_Test.Models
{
    public class ReciveStore
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }
}
