using StockBand.Models;
using System.ComponentModel.DataAnnotations;

namespace StockBand.ViewModel
{
    public class EditUserDto
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(20)]
        public string Name { get; set; }
        [Required]
        public bool Block { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password admin")]
        public string PasswordAdmin { get; set; }
        public IEnumerable<Role> ListOfRoles { get; set; }
        public Role Role { get; set; }
    }
}
