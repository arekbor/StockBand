using StockBand.Data;
using StockBand.Models;
using System.ComponentModel.DataAnnotations;

namespace StockBand.ViewModel
{
    public class CreateUserDto
    {
        [Required]
        [MaxLength(20)]
        [MinLength(5)]
        [DataType(DataType.Text)]
        [RegularExpression(@"^[0-9a-zA-Z]*$", ErrorMessage = Message.RegularExpName)]
        public string Name { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$", ErrorMessage = Message.RegularExpPwd)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }
    }
}
