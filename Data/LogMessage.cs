namespace StockBand.Data
{
    public static class LogMessage
    {
        public const string Code01 = "Logged in";
        public const string Code02 = "Logged out";

        public const string Code03 = "Registered";
        public const string Code04 = "Profile updated by admin";

        public const string Code05 = "Password changed";
        public static Func<Guid, string> Code06 = x => $"Unique link generated ({x})";

        public const string Code07 = "Persistent cookie setting";
        public const string Code08 = "Color changed";

        public const string Code09 = "Cookie removed";
        public const string Code10 = "Theme updated";

        public static Func<string, string> Code11 = x => $"Shared link: {x}";
        public static Func<Guid, string> Code12 = x => $"Unique link removed ({x})";

        public static Func<Guid, string> Code13 = x => $"Unique link refreshed ({x})";
        public static Func<Guid, int, string> Code14 = (x, t) => $"Default lenght of unique link ({x}) time updated to {t} minutes";

        public static Func<Guid, string> Code15 = x => $"Unique link removed automatically ({x})";
        public static Func<string, string> Code16 = x => $"Track '{x}' was uploaded";

        public const string Code17 = "Avatar updated";
        public const string Code18 = "Header updated";
        public static Func<string, string> Code19 = x => $"Track '{x}' was updated";
        public static Func<string, string> Code20 = x => $"Track '{x}' was deleted";
        public static Func<string, string> Code21 = x => $"Track '{x}' was converted";
    }
}
