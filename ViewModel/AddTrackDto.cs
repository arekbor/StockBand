using System.ComponentModel.DataAnnotations;

namespace StockBand.ViewModel
{
    public class AddTrackDto
    {
        [Required]
        [MaxLength(30)]
        public string Title { get; set; }
        [MaxLength(200)]
        public string Description { get; set; }
        public bool Private { get; set; }
        [Required]
        public IFormFile File { get; set; }
    }
}
