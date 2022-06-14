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
        public const string Code10 = "That username already exists";

        public const string Code11 = "No white space and special characters allowed";
        public const string Code12 = "Password must contain: Minimum 8 characters atleast 1 Alphabet and 1 Number";

        public const string Code13 = "Invalid old passowrd or new passwords are not matchning";
        public const string Code14 = "Authentication failed";

        public const string Code15 = "You do not have permission";
        public const string Code16 = "You do not have permission to access this page";

        public const string Code17 = "Empty result";
        public const string Code18 = "Color does not exist";

        public const string Code19 = "Settings can't be changed";
        public const string Code20 = "Theme does not exist";

        public const string Code21 = "Can't refresh this link";
        public const string Code22 = "Guid has expired or you do not have permission";

        public const string Code23 = "Guid not found";
        public static Func<string, string> Code24 = x => $"{x} DbContext seed successfully created";

        public static Func<string, string> Code25 = x => $"{x} DbContext has already been created";
        public const string Code26 = "Unsupported type";
        public const string Code27 = "That title already exists";
        public static Func<string, string> Code28 = x => $"This file is too large to be uploaded. Files larger than {x} MB are not supported";
        public const string Code29 = "Page not found";
        public const string Code30 = "You don't have enough space for uplouding this file";
        public static Func<string, string> Code31 = x => $"This image is too large to be uploaded. Images larger than {x} MB are not supported";
        public const string Code32 = "Avatar doesn't exists";
        public const string Code33 = "Header doesn't exists";
        public const string Code34 = "File doesn't exists";
        public const string Code35 = "Track doesn't exists";
        public const string Code36 = "Object doesn't exists";
        public const string Code37 = "That album name already exists";
        public const string Code38 = "Album limit exceeded";
        public const string Code39 = "Album doesn't exists";
        public const string Code41 = "Tracks album limit exceeded";
        public const string Code42 = "Unknow type of search profile";
        public static Func<string, string> Code43 = x => $"Are you sure you want to delete '{x}'?";
        public const string Code44 = "Cannot delete album. Album is not empty";
        public const string Code45 = "Accesses are incompatibility";
        public const string Code46 = "Please enter valid captcha";
        public const string Code47 = "Please enter the security code";
        public const string Code48 = "Security code";
        public const string Code49 = "An error occurred while converting the image";
        public const string Code50 = "Cannot clear this folder";
        public const string Code51 = "Your account is blocked";
    }
}
