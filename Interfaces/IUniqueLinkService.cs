using StockBand.Models;

namespace StockBand.Interfaces
{
    public interface IUniqueLinkService
    {
        Task<Guid> AddLink(string Type, int userId);
        Task<bool> DeleteLink(Guid guid);
        IQueryable<UniqueLink> GetAllLinks();
        Task<bool> VerifyLink(Guid guid);
        int GetCurrentExpireMintues();
    }
}