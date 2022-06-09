using Ganss.XSS;
using StockBand.Interfaces;

namespace StockBand.Services
{
    public class HtmlOperationService : IHtmlOperationService
    {
        private readonly IHtmlSanitizer _sanitizer;
        public HtmlOperationService(IHtmlSanitizer sanitizer)
        {
            _sanitizer = sanitizer;
        }
        public string SanitizeHtml(string text)
        {
            _sanitizer.KeepChildNodes = true;
            return _sanitizer.Sanitize(System.Web.HttpUtility.HtmlDecode(text));
        }
    }
}
