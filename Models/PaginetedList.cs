using Microsoft.EntityFrameworkCore;
using StockBand.Services;

namespace StockBand.Models
{
    public class PaginetedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public static int MaxPageSize { get; } = int.Parse(ConfigurationHelper.config.GetSection("MaxCountOfPagination").Value);
        public PaginetedList(List<T> items, int count, int pageIndex)
        {
            PageIndex = pageIndex;
            TotalCount = count;
            TotalPages = (int)Math.Ceiling(count / (double)MaxPageSize);
            this.AddRange(items);
        }
        public bool PreviousPage
        {
            get
            {
                return (PageIndex > 1);
            }
        }
        public bool NextPage
        {
            get
            {
                return (PageIndex < TotalPages);
            }
        }
        public static async Task<PaginetedList<T>> CreateAsync(IQueryable<T> source, int pageIndex)
        {
            if(pageIndex <= 0)
                pageIndex = 1;
            var count = await source.CountAsync();
            if(pageIndex > (int)Math.Ceiling(count / (double)MaxPageSize))
                pageIndex = (int)Math.Ceiling(count / (double)MaxPageSize);
            var items = await source.Skip((pageIndex - 1) * MaxPageSize).Take(MaxPageSize).ToListAsync();
            return new PaginetedList<T>(items, count, pageIndex);
            
        }
        public IEnumerable<int> Pages(int range)
        {
            var limit = Math.Min(Math.Max(1, TotalPages - 2 * range), Math.Max(1, PageIndex - range));
            return Enumerable.Range(limit, range * 2 + 1)
                .TakeWhile(p => p <= TotalPages)
                .ToList();
        }
        public string ActivePage(int page)
        {
            return page == PageIndex ? "active" : "";
        }
        public string PreviousDisabled()
        {
            return !PreviousPage ? "disabled" : "";
        }
        public string NextDisabled()
        {
            return !NextPage ? "disabled" : "";
        }
    }
}
