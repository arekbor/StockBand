using System.ComponentModel.DataAnnotations;

namespace StockBand.ViewModel
{
    public class UserLoginDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
