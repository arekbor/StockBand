using StockBand.Models;

namespace StockBand.Interfaces
{
    public interface IUniqueLinkService
    {
        Task<Guid> AddLink();
        Task<bool> DeleteLink(Guid guid);
        Task<IEnumerable<UniqueLink>> GetAllLinks();
        Task<bool> VerifyLink(Guid guid);
    }
}