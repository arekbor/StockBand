using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockBand.Models
{
    public class Track
    {
        [Key]
        public Guid Guid { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Lyrics { get; set; }
        public string Extension { get; set; }
        public DateTime DateTimeCreate { get; set; }
        public string TrackAccess { get; set; } = Data.TrackAccess.Access[1];
        public double Size { get; set; }
        public int? UserId { get; set; }
        public virtual User User { get; set; }
        public Guid? AlbumGuid { get; set; }
        public virtual Album Album { get; set; }
    }
}
