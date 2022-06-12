namespace StockBand.Interfaces
{
    public interface IImgService
    {
        public Task<bool> CompressImg(string path);
    }
}
