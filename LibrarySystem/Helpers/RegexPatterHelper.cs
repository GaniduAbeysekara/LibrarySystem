namespace LibrarySystem.Web.API.Helpers
{
    public static class RegexPatterHelper
    {

        public const string emailPattern = "^[a-z0-9._%+-]+@[a-z0-9.-]+\\.[a-z]{2,}$";
        public const string phoneNoPattern = "^\\(?([0-9]{3})\\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$";
        public const string passwordPattern = "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$";
        // Add more messages as needed...
    }
}
