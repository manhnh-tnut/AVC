using System.ComponentModel.DataAnnotations;

namespace AVC.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string Gen { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
