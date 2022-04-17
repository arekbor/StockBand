using StockBand.Models;
using StockBand.ViewModel;

namespace StockBand.Interfaces
{
    public interface IUniqueLinkService
    {
        Task<Guid> AddLink(string type, int userId, string controller, string action);
        Task<bool> DeleteLink(UniqueLink link);
        IQueryable<UniqueLink> GetAllLinks();
        Task<UniqueLink> GetUniqueLink(Guid guid);
        Task<bool> RefreshUrl(UniqueLink link);
        Task<bool> SetMinutes(UniqueLink link, int minutes);
        Task<string> ShowLink(UniqueLink link);
        bool VerifyAuthorId(UniqueLink link);
        bool VerifyLink(UniqueLink link);

    }
}