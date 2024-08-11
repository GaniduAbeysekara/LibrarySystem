namespace LibrarySystem.Web.API.Helpers
{
    public static class ResponseMessagesHelper
    {

        //AuthController Response messages
        public const string UserCreatedSuccess = "User created successfully.";
        public const string UserRegistrationFailed = "Failed to register user.";
        public const string LoggedInSuccessfully = "Logged in Successfully.";
        public const string NoUsersAvailable = "No Users available.";
        public const string NoUserFound = "No User/Users found matching the keyword.";
        public const string InvalidToken = "Invalid token";
        public const string UserNotExist = "This user does not exist";
        public const string UserDeleteSuccess = "User deleted successfully.";
        public const string UserNotRegistered = "This user is not registered";
        public const string ForbiddenAccess = "You do not have permission to access details of other users.";
        public const string ForbiddenDelete = "You do not have permission to delete other users.";
        public const string TokenRequired = "Token is required.";
        public const string LoggedOutSuccessfully = "Logged out successfully.";
        public const string InternalServerError = "Internal server error. Please try again later.";
        public const string NoFieldsUpdated = "No fields were updated.";


        //AuthService Response messages
        public const string EnterLowerCaseEmail = "Please enter a valid email address in lower case";
        public const string EnterValiedEmail = "Please enter a valid Email Address!";
        public const string EnterValiedPhoneNumber = "Please enter a valid Phone Number!";
        public const string EnterValiedPassword = "Password must be at least 8 to 15 characters. It contains at least one Upper case, Lower Case, numbers, and Special Characters.";
        public const string PasswordNotMatch = "Passwords do not match!";
        public const string EmailAlreadyExists = "User with this email already exists.";
        public const string EmailNotRegistered = "This email is not registered";
        public const string IncorrectPassword = "Incorrect password!";
        public const string InvalidUserId = "User deletion requires a valid User ID.";
        public const string UnableToDeleteOwnAccount = "Unable to delete account. You cannot delete your own account.";
        public const string UnableToDeleteAdminAccount = "Sorry...Admin cannot delete an Admin";
        public const string RequestBodyMissing = "The request body is missing.";
        public const string InvalidSearchCriteria = "Invalid search criteria format.";
        public const string SearchCharacters = "Search Using 3 or more characters";



        // Add more messages as needed...
    }
}
