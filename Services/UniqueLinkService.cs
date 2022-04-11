using Microsoft.EntityFrameworkCore;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Models;
using System.Security.Claims;

namespace StockBand.Services
{
    public class UniqueLinkService : IUniqueLinkService
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public UniqueLinkService(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public async Task<Guid> AddLink(string type, int userId)
        {
            var minutes = GetCurrentExpireMintues();
            var uniqueLink = new UniqueLink()
            {
                Guid = Guid.NewGuid(),
                DateTimeExpire = DateTime.Now.AddMinutes(minutes),
                Type = type,
                UserId = userId
            };
            await _applicationDbContext
                .UniqueLinkDbContext
                .AddAsync(uniqueLink);
            return uniqueLink.Guid;
        }

        public async Task<bool> DeleteLink(Guid guid)
        {
            var uniqueLink = await _applicationDbContext
                .UniqueLinkDbContext
                .FirstOrDefaultAsync(x => x.Guid == guid);
            if (uniqueLink is null)
            {
                return false;
            }
            _applicationDbContext
                .UniqueLinkDbContext
                .Remove(uniqueLink);
            _applicationDbContext.SaveChanges();
            return true;
        }

        public IQueryable<UniqueLink> GetAllLinks()
        {
            var uniqueLinks =  _applicationDbContext
                .UniqueLinkDbContext
                .AsQueryable();
            if (uniqueLinks is null)
                return null;
            return uniqueLinks;
        }

        public int GetCurrentExpireMintues()
        {
            if (!int.TryParse(ConfigurationManager.Configuration["UniqueLinkExpire"], out var minutes))
                throw new ArgumentException("Parsing UniqueLinkExpire fail");
            return minutes;
        }
        public async Task<bool> VerifyLink(Guid guid)
        {
            var verifyLink = await _applicationDbContext
                .UniqueLinkDbContext
                .FirstOrDefaultAsync(x => x.Guid == guid);
            if (verifyLink is null)
                return false;
            if (verifyLink.DateTimeExpire >= DateTime.Now)
                return true;
            return false;
        }
    }
}
