using StockBand.ViewModel;

namespace StockBand.Interfaces
{
    public interface IAlbumService
    {
        public Task<bool> AddAlbumAsync(AddAlbumDto addAlbumDto);
    }
}
