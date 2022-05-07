using StockBand.Models;
using StockBand.ViewModel;

namespace StockBand.Interfaces
{
    public interface ITrackService
    {
        public Task<bool> AddTrack(AddTrackDto dto);
        public IQueryable<Track> GetAllUserTracksAsync();
        public Task<Track> GetTrack(Guid guid);
        public bool VerifyAccessTrack(Track track);
        public bool VerifyAuthorTrack(Track track);
    }
}
