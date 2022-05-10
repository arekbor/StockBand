using System.ComponentModel.DataAnnotations;

namespace StockBand.Models
{
    public class UserLog
    {
        [Key]
        public Guid Guid { get; set; }
        public string Action { get; set; }
        public DateTime CreatedDate { get; set; }
        public virtual User User { get; set; }
        public int UserId { get; set; }
    }
}
