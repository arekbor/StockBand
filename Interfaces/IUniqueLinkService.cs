using StockBand.Models;

namespace StockBand.Interfaces
{
    public interface IUniqueLinkService
    {
        Task<Guid> AddLink(string type, int userId, string controller, string action);
        Task<bool> DeleteLink(UniqueLink link);
        Task<int> GetActualMinutes(Guid guid);
        IQueryable<UniqueLink> GetAllLinks();
        Task<UniqueLink> GetUniqueLink(Guid guid);
        Task<bool> RefreshUrl(UniqueLink link);
        Task<bool> SetMinutesUrl(Guid guid, int minutes);
        Task<string> ShowLink(UniqueLink link);
        bool VerifyAuthorId(UniqueLink link);
        bool VerifyLink(UniqueLink link);

    }
}