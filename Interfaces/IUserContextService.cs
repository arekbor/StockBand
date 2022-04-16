using System.Security.Claims;

namespace StockBand.Interfaces
{
    public interface IUserContextService
    {
        public ClaimsPrincipal GetUser();
        public int GetUserId();
    }
}
