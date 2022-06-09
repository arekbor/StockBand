using StockBand.Models;
using StockBand.ViewModel;

namespace StockBand.Interfaces
{
    public interface ILinkService
    {
        Task<Guid> AddLink(int userId, string controller, string action);
        Task<bool> DeleteLink(Link link);
        IQueryable<Link> GetAllLinks();
        Task<Link> GetUniqueLink(Guid guid);
        Task<bool> RefreshUrl(Link link);
        Task<bool> SetMinutes(Link link, int minutes);
        Task<string> ShowLink(Link link);
        bool VerifyAuthorId(Link link);
        bool VerifyLink(Link link);
    }
}