using System.ComponentModel.DataAnnotations;

namespace atm.Models
{
    public class TransactionType
    {
        [Key]
        public int TransactionTypeID { get; set; }

        [Required]
        public string TransactionTypeName { get; set; }
    }
}
