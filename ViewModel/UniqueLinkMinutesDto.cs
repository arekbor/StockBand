using StockBand.Models;
using System.ComponentModel.DataAnnotations;

namespace StockBand.ViewModel
{
    public class UniqueLinkMinutesDto
    {
        public Guid Guid { get; set; }
        public string Type { get; set; }
        public virtual User User { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        //TODO to ustaw max minutes z appsettings.json
        [Required]
        [Range(0, 60)]
        public int Minutes { get; set; }
    }
}
