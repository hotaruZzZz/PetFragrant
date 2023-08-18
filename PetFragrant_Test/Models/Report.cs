using System.ComponentModel.DataAnnotations.Schema;

namespace PetFragrant_Test.Models
{
    public class Report
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string id { get; set; }
        public string CustomerId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public virtual Customer Customer { get; set; }
    }
}
