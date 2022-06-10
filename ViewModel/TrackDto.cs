using StockBand.Models;

namespace StockBand.ViewModel
{
    public class TrackDto
    {
        public Guid Guid { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Lyrics { get; set; }
        public string Extension { get; set; }
        public DateTime DateTimeCreate { get; set; }
        public string TrackAccess { get; set; }
        public virtual User User { get; set; }
        public double Size { get; set; }
        public int UserId { get; set; }
        public Guid AlbumGuid { get; set; }
    }
}
