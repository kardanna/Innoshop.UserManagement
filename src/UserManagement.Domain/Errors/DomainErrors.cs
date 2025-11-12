using UserManagement.Domain.Shared;

namespace UserManagement.Domain.Errors;

public static class DomainErrors
{
    public static class User
    {

    }

    public static class Register
    {
        public static readonly Error EmailAlreadyInUse = new(
            "Register.EmailAlreadyInUse",
            "Specified email is already in use."
        );

        public static readonly Error IllegalAge = new(
            "Register.IllegalAge",
            "User must be older to register."
        );
    }
    
    public static class Email
    {
        public static readonly Error InvalidFormat = new(
            "Email.InvalidFormat",
            "Invalid email."
        );
    }

    public static class Login
    {
        public static readonly Error WrongEmailOrPassword = new(
            "Login.WrongEmailOrPassword",
            "Either user with provided email is not registered or the password is wrong."
        );

        public static readonly Error TooManyAttempts = new(
            "Login.TooManyAttempts",
            "Too many login attempts reached. Try again later."
        );

        public static readonly Error EmailUnverified = new(
            "Login.EmailUnverified",
            "Can not login with unverified account."
        );
    }

    public static class RefreshToken
    {
        public static readonly Error NotFound = new(
            "RefreshToken.NotFound",
            "Refresh token not found."
        );

        public static readonly Error Expired = new(
            "RefreshToken.Expired",
            "Refresh token expired."
        );
    }
}