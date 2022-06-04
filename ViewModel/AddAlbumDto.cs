using System.ComponentModel.DataAnnotations;

namespace StockBand.ViewModel
{
    public class AddAlbumDto
    {
        [Required]
        [MaxLength(30)]
        public string Name { get; set; }
        [MaxLength(120)]
        public string Description { get; set; }
    }
}
