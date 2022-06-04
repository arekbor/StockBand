using StockBand.Services;

namespace StockBand.Data
{
    public static class UserPath
    {
        private static string ContentPath = $"{ConfigurationHelper.config.GetSection("UserProfileContentPath").Value}";
        private static string UserPrefix = $"{ConfigurationHelper.config.GetSection("UserProfilePrefixFolder").Value}";
        private static string TrackFolderName = $"{ConfigurationHelper.config.GetSection("UserProfileTracksFolderName").Value}";
        private static string ImagesFolderName = $"{ConfigurationHelper.config.GetSection("UserProfileImagesFolderName").Value}";
        private static string AlbumsFolderName = $"{ConfigurationHelper.config.GetSection("UserProfileAlbumsFolderName").Value}";

        public static Func<string, string> UserTracksPath = x => $"{ContentPath}{UserPrefix}{x}/{TrackFolderName}";
        public static Func<string, string> UserImagesPath = x => $"{ContentPath}{UserPrefix}{x}/{ImagesFolderName}";
        public static Func<string, string> UserAlbumsPath = x => $"{ContentPath}{UserPrefix}{x}/{AlbumsFolderName}";
    }
}
