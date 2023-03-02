using System.ComponentModel.DataAnnotations;

namespace atm.Models
{
    public class Transfer
    {
        [Key]
        public int TransferID { get; set; }

        [Required]
        public int TransactionID { get; set; }

        [Required]
        public string SenderEmail  { get; set; }

        [Required]
        public string RecipientAccount { get; set; }

        [Required]
        public float Amount { get; set; }

        [Required]
        public DateTime TransferDate { get; set; }
    }
}
