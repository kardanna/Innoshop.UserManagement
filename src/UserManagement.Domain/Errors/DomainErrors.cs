using UserManagement.Domain.Shared;

namespace UserManagement.Domain.Errors;

public static class DomainErrors
{
    public static class User
    {
        public static readonly Error NotFound = new(
            "User.NotFound",
            "Requested user not found."
        );

        public static readonly Error Deactivated = new(
            "User.Deactivated",
            "User is deactivated."
        );
    }

    public static class Authentication
    {
        public static readonly Error InvalidSubjectClaim = new(
            "Authentication.InvalidSubjectClaim",
            "Token has no subject claim or claim value is invalid."
        );

        public static readonly Error InvalidJwtIdClaim = new(
            "Authentication.InvalidJwtIdClaim",
            "Token has no token ID claim or claim value is invalid."
        );


        public static readonly Error AccessTokenNotFound = new(
            "Authentication.AccessTokenNotFound",
            "Requested access token not found."
        );
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

    public static class EmailVerification
    {
        public static readonly Error CodeExpiredOrNotFound = new(
            "EmailVerification.CodeExpiredOrNotFound",
            "Verification code is invalid or has expired."
        );
    }

    public static class EmailChange
    {
        public static readonly Error EmailAlreadyInUse = new(
            "EmailChange.EmailAlreadyInUse",
            "Specified email is already in use."
        );

        public static readonly Error TooOften = new(
            "EmailChange.TooOften",
            "Policy on number of successful email changes in a set period of time is violated."
        );

        public static readonly Error TheSameEmail = new(
            "EmailChange.TheSameEmail",
            "New email must differ from current email."
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