using System.ComponentModel.DataAnnotations;

namespace AVC.ViewModels
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
