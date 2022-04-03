namespace StockBand.Models
{
    public class UserLog
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public virtual User User { get; set; }
        public int UserId { get; set; }
    }
}
