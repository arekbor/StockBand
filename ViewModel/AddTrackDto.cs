using StockBand.Data;
using System.ComponentModel.DataAnnotations;

namespace StockBand.ViewModel
{
    public class AddTrackDto
    {
        [Required]
        [MaxLength(50)]
        [MinLength(5)]
        public string Title { get; set; }
        [MaxLength(200)]
        public string Description { get; set; }
        [Required]
        public string TrackAccess { get; set; }
        [Required]
        public IFormFile File { get; set; }
    }
}
