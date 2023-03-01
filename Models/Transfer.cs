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
        public int SenderID  { get; set; }

        [Required]
        public int ReceiverID { get; set; }

        [Required]
        public float Amount { get; set; }

        [Required]
        public DateTime TransferDate { get; set; }
    }
}
