using StockBand.Models;
using StockBand.ViewModel;

namespace StockBand.Interfaces
{
    public interface IAlbumService
    {
        public Task<bool> AddAlbumAsync(AddAlbumDto addAlbumDto);
        public IQueryable<Album> GetAllUserAlbums(int id);
        public Task<int> GetCountOfAlbumsByUserId(int userId);
        public Task<Album> GetAlbumByUserame(string name);
        public Task<Album> GetAlbum(Guid guid);
        public Task<IEnumerable<Track>> GetAlbumTracks(Album album);
        public Task<int> GetCountOfAlbumTracks(Album album);
        public Task<bool> EditAlbum(Guid guid, EditAlbumDto editAlbum);
        public Task<bool> RemoveAlbum(Album album);
        public Task<IEnumerable<Album>> GetSpecificQuantityOfAlbums(int userId, int quantity);
    }
}
