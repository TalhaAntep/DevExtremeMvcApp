using System.ComponentModel.DataAnnotations;

namespace DevExtremeMvcApp2.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage ="Please enter a valid email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage ="Please enter a valid password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember Me?")]
        public bool RememberMe { get; set; }
    }

}


