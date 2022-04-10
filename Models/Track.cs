namespace StockBand.Models
{
    public class Track
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime DateTimeCreate { get; set; }
        public int UserId { get; set; }
        public long Plays { get; set; }
        public bool Privacy { get; set; }
        public virtual User User { get; set; }
    }
}
