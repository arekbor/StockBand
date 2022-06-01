using System.ComponentModel.DataAnnotations;

namespace StockBand.Models
{
    public class Album
    {
        [Key]
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public virtual User User { get; set; }
        public int UserId { get; set; }
    }
}
