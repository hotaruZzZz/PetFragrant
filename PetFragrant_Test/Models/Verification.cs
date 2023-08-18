using System.ComponentModel.DataAnnotations;

namespace PetFragrant_Test.Models
{
    public class Verification
    {
        [Key]
        public string Email { get; set; }
        public string VerificationCode { get; set; }
    }
}
