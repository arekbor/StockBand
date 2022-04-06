using StockBand.Interfaces;
using StockBand.Models;

namespace StockBand.Services
{
    public static class UniqueLinkService
    {
        private static List<UniqueLink> _repository = new List<UniqueLink>();

        public static Guid AddLink()
        {
            var uniqueLink = new UniqueLink()
            {
                Guid = Guid.NewGuid(),
                DateTimeExpire = DateTime.UtcNow
            };
            _repository.Add(uniqueLink);
            return uniqueLink.Guid;
        }

        public static bool DeleteLink(Guid guid)
        {
            var uniqueLink = _repository
                .FirstOrDefault(x => x.Guid == guid);
            if(uniqueLink is null)
                return false;
            _repository.Remove(uniqueLink);
            return true;
        }

        public static IEnumerable<UniqueLink> GetAllLinks()
        {
            var uniqueLinks = _repository
                .ToList();
            if (uniqueLinks is null)
                return null;
            return uniqueLinks;
        }

        public static bool VerifyLink(Guid guid)
        {
            var verifyLink = _repository
                .FirstOrDefault(x => x.Guid == guid);
            if (verifyLink is null)
                return false;
            if(!int.TryParse(ConfigurationManager.Configuration["UniqueLinkExpire"], out var result))
                return false;
            if (verifyLink.DateTimeExpire.AddMinutes(result) >= DateTime.Now)
                return true;
            return false;
        }
    }
}
