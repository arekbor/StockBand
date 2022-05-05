using StockBand.ViewModel;

namespace StockBand.Interfaces
{
    public interface ITrackService
    {
        public Task<bool> AddTrack(AddTrackDto dto);
    }
}
