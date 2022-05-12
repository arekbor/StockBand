using StockBand.Models;
using StockBand.ViewModel;

namespace StockBand.Interfaces
{
    public interface ITrackService
    {
        public Task<bool> AddTrack(AddTrackDto dto);
        public IQueryable<Track> GetAllUserTracksAsync(int id);
        public Task<Track> GetTrack(Guid guid);
        public bool VerifyAccessTrack(Track track);
    }
}
