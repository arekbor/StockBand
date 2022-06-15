using StockBand.Data;
using System.ComponentModel.DataAnnotations;

namespace StockBand.ViewModel
{
    public class AddTrackDto
    {
        [Required]
        [MaxLength(50)]
        public string Title { get; set; }
        [MaxLength(250)]
        public string Description { get; set; }
        [Required]
        [Display(Name = "Access track")]
        public string TrackAccess { get; set; }
        [Required]
        public IFormFile File { get; set; }
        [Display(Name = "Choose album")]
        public string AlbumName { get; set; }
        [Display(Name = "Asign album")]
        public bool IsAlbumSelectedToChoose { get; set; }
    }
}
