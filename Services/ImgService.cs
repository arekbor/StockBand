using StockBand.Interfaces;
using TinyPng;

namespace StockBand.Services
{
    public class ImgService : IImgService
    {
        private readonly IConfiguration _configuration;
        public ImgService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<bool> CompressImg(string path)
        {
            var png = new TinyPngClient(_configuration["TinyPngClientAPIKey"]);
            try
            {
                using (var compressImageTask = png.Compress(path))
                {
                    var compressedImage = await compressImageTask.Download();
                    if (!File.Exists(path))
                        return false;
                    File.Delete(path);
                    await compressedImage.SaveImageToDisk(path);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
