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
        //TODO to ustaw max minutes z appsettings.json
        public int Minutes { get; set; }
        public string ReturnController { get; set; }
        public string ReturnAction { get; set; }
        public int ReturnPage { get; set; }
    }
}
