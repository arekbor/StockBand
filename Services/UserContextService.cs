using StockBand.Interfaces;
using System.Security.Claims;

namespace StockBand.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public ClaimsPrincipal GetUser()
        {
            return _httpContextAccessor?.HttpContext?.User as ClaimsPrincipal;
        }

        public int GetUserId()
        {
            return int.Parse(GetUser().FindFirst(x => x.Type == ClaimTypes.NameIdentifier).Value);
        }
    }
}
