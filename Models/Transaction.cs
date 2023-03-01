using System.ComponentModel.DataAnnotations;

namespace atm.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionID { get; set; }

        [Required]
        public int TransactionTypeID { get; set; }

        [Required]
        public int TransactionAmount { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; }

    }
}
