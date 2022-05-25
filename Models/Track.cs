using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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
        public int UserId { get; set; }
        public virtual User User { get; set; }
        //TODO implement this
        public bool IsDownloadle { get; set; }
        public double Size { get; set; }
    }
}
