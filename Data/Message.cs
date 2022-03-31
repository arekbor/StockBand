namespace StockBand.Data
{
    public static class Message
    {
        public const string LinkExpired = "Link you followed has expired";
        public const string GuidExpired = "Guid has expired";
        public const string CannotCreateWhenLogged = "Cannot create a new account when you're logged";
        public const string InvalidUsrPwd = "Invalid username or passowrd";
        public const string WrongAdmPwd = "Wrong admin password";
        public const string UserNotEx = "User does not exist";
        public const string RoleNotEx = "Role does not exist";
        public const string UsrAlreadyEx = "Than username already exists";
        public const string PwdNotMatch = "Passwords are not matching";
        public const string PwdUsr = "Password cannot be the same as username";
        public const string RegularExpName = "No white space and special characters allowed";
        public const string RegularExpPwd = "Password must contain: Minimum 8 characters atleast 1 Alphabet and 1 Number";
    }
    public static class SystemMessage
    {
        public static Func<string, string> DbContextSuccess = x => $"{x} DbContext seed successfully created";
        public static Func<string, string> DbContextAlreadyCreated = x => $"{x} DbContext has already been created";
    }
}
