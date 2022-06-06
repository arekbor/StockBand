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
        public string TrackAccess { get; set; }
        public int UserId { get; set; }
    }
}
