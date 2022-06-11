using System.ComponentModel.DataAnnotations;

namespace StockBand.ViewModel
{
    public class EditTrackDto
    {
        public Guid Guid { get; set; }
        [Required]
        [MaxLength(50)]
        public string Title { get; set; }
        [MaxLength(200)]
        public string Description { get; set; }
        [Required]
        [Display(Name = "Access track")]
        public string TrackAccess { get; set; }
        [MaxLength(1000)]
        public string Lyrics { get; set; }
        public int UserId { get; set; }
        [Display(Name = "Access lyrics")]
        public string LyricsAccess { get; set; }
    }
}
