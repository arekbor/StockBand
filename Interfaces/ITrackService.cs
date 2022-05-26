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
        public Task<Guid> GetGuidTrackByTitle(string title);
        public Task<int> GetTracksCountByUserId(int id);
        public Task<string> GetLastUploadTrackNameByUserId(int id);
        public Task<double> GetTotalSizeOfTracksByUserId(int id);
        public Task<bool> EditTrack(Guid guid, EditTrackDto track);
    }
}
