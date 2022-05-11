namespace StockBand.ViewModel
{
    public class UserDto
    {
        public string Name { get; set; }
        public DateTime CreatedTime { get; set; }
        public string Color { get; set; }
        public string Theme { get; set; }
        public int LimitTracks { get; set; }
        public int TotalTracks { get; set; }
        public string PathAvatar { get; set; }
        public string PathHeader { get; set; }
    }
}
