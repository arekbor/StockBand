using StockBand.Models;

namespace StockBand.ViewModel
{
    public class UniqueLinkMinutesDto
    {
        public Guid Guid { get; set; }
        public string Type { get; set; }
        public virtual User User { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public int Minutes { get; set; }
    }
}
