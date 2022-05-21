using StockBand.Data;

namespace StockBand.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string HashPassword { get; set; }
        public string Role { get; set; } = UserRoles.Roles[0];
        //TODO implement in middleware Block user
        public bool Block { get; set; } = false;
        public DateTime CreatedTime { get; set; }
        public string Color { get; set; } = UserColor.Colors[0];
        public string Theme { get; set; } = UserTheme.Themes[0];
        public bool RememberMe { get; set; } = false;
        public int TotalTracks { get; set; }
        public bool IsAvatarUploaded { get; set; } = false;
        public string AvatarType { get; set; }
    }
}