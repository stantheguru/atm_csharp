using System.ComponentModel.DataAnnotations;

namespace atm.Models
{
    public class Login
    {
        
        [Required]
        public string Email { get; set; }


        [Required]
        public int PIN { get; set; }




    }
}
