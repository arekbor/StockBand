using StockBand.Models;
using StockBand.ViewModel;

namespace StockBand.Interfaces
{
    public interface IAlbumService
    {
        public Task<bool> AddAlbumAsync(AddAlbumDto addAlbumDto);
        public IQueryable<Album> GetAllUserAlbums(int id);
        public Task<int> GetCountOfAlbums(int userId);
        public Task<Album> GetAlbumByName(string name);
    }
}
