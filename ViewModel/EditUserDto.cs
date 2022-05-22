using System.ComponentModel.DataAnnotations;

namespace StockBand.ViewModel
{
    public class EditUserDto
    {
        [Required]
        public IFormFile Image { get; set; }
    }
}
