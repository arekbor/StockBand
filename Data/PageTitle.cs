namespace StockBand.Data
{
    public static class PageTitle
    {
        public const string PageTitle01 = "400 bad request";
        public const string PageTitle02 = "Exception";
        public const string PageTitle03 = "403 forbidden";
        public const string PageTitle04 = "500 internal server error";
        public const string PageTitle05 = "404 not found";
        public const string PageTitle06 = "Edit avatar";
        public const string PageTitle07 = "Change color";
        public const string PageTitle08 = "Change password";
        public const string PageTitle09 = "Change theme";
        public const string PageTitle10 = "Create account";
        public const string PageTitle11 = "Edit header";
        public const string PageTitle12 = "Login in";
        public const string PageTitle13 = "User logs";
        public const string PageTitle14 = "User settings";
        public static Func<string, string> PageTitle15 = x => $"Edit user {x}";
        public const string PageTitle16 = "All logs";
        public const string PageTitle17 = "Users panel";
        public const string PageTitle18 = "Home";
        public const string PageTitle19 = "Upload track";
        public const string PageTitle20 = "All tracks";
        public static Func<string, string> PageTitle21 = x => $"Edit track {x}";
        public const string PageTitle22 = "Edit expire time of link";
        public const string PageTitle23 = "Link panel";
        public const string PageTitle24 = "Share link";
        public const string PageTitle25 = "Add album";
    }
}
