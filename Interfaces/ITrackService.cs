using StockBand.Models;
using StockBand.ViewModel;

namespace StockBand.Interfaces
{
    public interface ITrackService
    {
        public Task<bool> AddTrack(AddTrackDto dto);
        public IQueryable<Track> GetAllUserTracks(int id);
        public Task<Track> GetOnlyTrack(Guid guid);
        public Task<Track> GetWholeTrack(Guid guid);
        public bool VerifyAccess(string property, int userId);
        public Task<Guid> GetGuidTrackByTitle(string title);
        public Task<int> GetTracksCountByUserId(int id);
        public Task<string> GetLastUploadTrackNameByUserId(int id);
        public Task<double> GetTotalSizeOfTracksByUserId(int id);
        public Task<bool> EditTrack(Guid guid, EditTrackDto track);
        public Task<bool> DeleteTrack(Track track);
        public IQueryable<Track> GetAllTracks();
        public bool IsTrackExtWav(Track track);
        public Task<bool> WavToMp3(Track track);
        public bool IsTrackFileExists(Track track);
        public bool IsAccessTrackAndLyricsAreCompatible(string trackAccess, string lyricsAccess);
        public Task<bool> IsUserTracksContextIsCompatibile(int userId);
    }
}
