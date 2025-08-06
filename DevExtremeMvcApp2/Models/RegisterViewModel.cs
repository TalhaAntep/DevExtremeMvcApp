using System.ComponentModel.DataAnnotations;

namespace DevExtremeMvcApp2.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage ="Please enter a valid email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage ="Please enter a valid password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }

}


