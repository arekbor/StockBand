namespace StockBand.Data
{
    public static class Message
    {
        public const string Code01 = "Link you followed has expired";
        public const string Code02 = "Cannot create a new account when you're logged";

        public const string Code03 = "Invalid username or passowrd";
        public const string Code04 = "Wrong admin password";

        public const string Code05 = "Guid has expired";
        public const string Code06 = "Password cannot be the same as username";

        public const string Code07 = "Passwords are not matching";
        public const string Code08 = "Role does not exist";

        public const string Code09 = "User does not exist";
        public const string Code10 = "Than username already exists";

        public const string Code11 = "No white space and special characters allowed";
        public const string Code12 = "Password must contain: Minimum 8 characters atleast 1 Alphabet and 1 Number";

        public const string Code13 = "Invalid old passowrd or new passwords are not matchning";
        public const string Code14 = "Authentication failed";

        public const string Code15 = "You do not have permission";
        public const string Code16 = "You do not have permission to access this page";
    }
    public static class SystemMessage
    {
        public static Func<string, string> Code01 = x => $"{x} DbContext seed successfully created";
        public static Func<string, string> Code02 = x => $"{x} DbContext has already been created";
    }
}
