using System.ComponentModel.DataAnnotations;

namespace BeachSAAPI.Models
{
    public class User
    {
        [Key]
        public int ID { get; set; }
        public string IdType { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } //Admin, and Customer
    }
}
