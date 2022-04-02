using StockBand.Data;

namespace StockBand.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string HashPassword { get; set; }
        public string Role { get; set; } = UserRoles.Roles[0];
        public bool Block { get; set; } = false;
    }
}
