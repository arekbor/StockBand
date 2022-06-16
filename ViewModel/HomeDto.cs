using StockBand.Models;

namespace StockBand.ViewModel
{
    public class HomeDto
    {
        public string TypeSearch { get; set; }
        public PaginetedList<dynamic> Library { get; set; }
    }
}
