using StockBand.Models;

namespace StockBand.Interfaces
{
    public interface IUniqueLinkService
    {
        Task<Guid> AddLink(string type, int userId,string controller, string action);
        Task<bool> DeleteLink(Guid guid);
        IQueryable<UniqueLink> GetAllLinks();
        Task<bool> VerifyLink(Guid guid);
        Task<string> ShowLink(Guid guid);
        Task<bool> VerifyAuthorId(Guid guid);
        Task<bool> RefreshUrl(Guid guid);
    }
}