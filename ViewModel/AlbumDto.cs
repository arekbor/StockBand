using StockBand.Models;

namespace StockBand.ViewModel
{
    public class AlbumDto
    {
        public Guid Guid { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateTimeCreate { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public IEnumerable<Track> Tracks { get; set; }
        public int CountTracks { get; set; }
    }
}
