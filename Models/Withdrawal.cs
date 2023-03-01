using System.ComponentModel.DataAnnotations;

namespace atm.Models
{
    public class Withdrawal
    {
        [Key]
        public int WithdrawalID { get; set; }

        [Required]
        public int TransactionID { get; set; }

        [Required]
        public int CustomerID { get; set; }

        [Required]
       
        public float Amount { get; set; }

        [Required]
        public DateTime WithdrawalDate { get; set; }
    }
}
