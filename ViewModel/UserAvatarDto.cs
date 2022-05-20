using System.ComponentModel.DataAnnotations;

namespace StockBand.ViewModel
{
    public class UserAvatarDto
    {
        [Display(Name = "Avatar")]
        [Required]
        public IFormFile AvatarFile { get; set; }
    }
}
