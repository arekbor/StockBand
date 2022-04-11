using StockBand.Data;
using System.ComponentModel.DataAnnotations;

namespace StockBand.Models
{
    public class UniqueLink
    {
        [Key]
        public Guid Guid { get; set; }
        public DateTime DateTimeExpire { get; set; }
        public string Type { get; set; } 
    }
}
