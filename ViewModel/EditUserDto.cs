using System.ComponentModel.DataAnnotations;

namespace StockBand.ViewModel
{
    public class EditUserDto
    {
        [Display(Name = "Avatar")]
        [Required]
        public IFormFile AvatarFile { get; set; }
    }
}
