using StockBand.Services;
using System.ComponentModel.DataAnnotations;

namespace StockBand.ViewModel
{
    public class AddAlbumDto
    {
        [Required]
        [MaxLength(30)]
        public string Title { get; set; }
        [MaxLength(250)]
        public string Description { get; set; }
    }
}
