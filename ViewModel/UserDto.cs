using StockBand.Models;

namespace StockBand.ViewModel
{
    public class UserDto
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public DateTime CreatedTime { get; set; }
        public string Color { get; set; }
        public string Theme { get; set; }
        public double TotalSizeOfTracks { get; set; }
        public int TotalTracks { get; set; }
        public bool IsAvatarUploaded { get; set; }
        public bool IsHeaderUploaded { get; set; }
        public PaginetedList<Album> Albums { get; set; }
        public string LastUpload { get; set; }
    }
}
