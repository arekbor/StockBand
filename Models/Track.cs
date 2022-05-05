namespace StockBand.Models
{
    public class Track
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateTimeCreate { get; set; }
        public long PlaysCount { get; set; } = 0;
        public bool Private { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}
