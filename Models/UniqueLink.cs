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
        public virtual User User { get; set; }
        public int UserId { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public int Minutes { get; set; } = 5;
    }
}
