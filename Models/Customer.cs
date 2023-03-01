using System.ComponentModel.DataAnnotations;

namespace atm.Models
{
    public class Customer
    {
        [Key]
        public int CustomerID { get; set; }

        [Required]
        public int ID { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string KRAPIN { get; set; }

        [Required]
        public string AccountName { get; set; }

        [Required]
        public string AccountNumber { get; set; }

        [Required]
        public int PIN { get; set; }

        [Required]
        public string MobileNumber { get; set; }

        [Required]
        public float Limit { get; set; }

        [Required]
        public float AvailableBalance { get; set; }

        [Required]
        public float ActualBalance { get; set; }

        [Required]
        public DateTime RegistrationDate { get; set; }



    }
}
