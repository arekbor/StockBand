using Microsoft.EntityFrameworkCore;

namespace StockBand.Models
{
    public class PaginetedList<T>: List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; set; }
        public PaginetedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
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
        public static async Task<PaginetedList<T>> CreateAsync(IQueryable<T> source,int pageIndex,int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginetedList<T>(items, count, pageIndex, pageSize);
        }
        public IEnumerable<int> Pages(int range)
        {
            var limit = Math.Min(Math.Max(1, TotalPages - 2 * range), Math.Max(1, PageIndex - range));
            return Enumerable.Range(limit, range * 2 + 1)
                .TakeWhile(p => p <= TotalPages)
                .ToList();
        }
    }
}
