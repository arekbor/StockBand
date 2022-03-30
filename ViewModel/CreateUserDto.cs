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
        [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "No white space and special characters allowed")]
        public string Name { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])|(?=.*[A-Z])|(?=.*\d)|(?=.*[^a-zA-Z\d])$", ErrorMessage = "Password should have atleast one lowercase | atleast one uppercase, should have atleast one number, should have atleast one special character")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        public Guid GuidValidation { get; set; }
    }
}
