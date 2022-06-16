namespace StockBand.Data
{
    public static class InterfaceContext
    {
        public const string Code01 = "View my profile";
        public const string Code02 = "Account logs";
        public const string Code03 = "Account details";
        public static Func<string, string> Code04 = x => $"Log out of account: {x}";
        public const string Code05 = "Change my password";
        public const string Code06 = "Change my color";
        public const string Code07 = "Change my theme";
        public static Func<string, string> Code08 = x => $"Total tracks: {x}";
        public static Func<string, string> Code09 = x => $"Joined: {x}";
        public static Func<string, string,string> Code10 = (x,y) => $"{x}MB used of {y}MB";
        public static Func<string, string> Code11 = x => $"Last upload: {x}";
    }
}
