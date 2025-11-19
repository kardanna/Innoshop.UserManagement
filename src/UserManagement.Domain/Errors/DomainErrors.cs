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

    public static class PasswordChange
    {
        public static readonly Error EmptyOrWrongPassword = new(
            "PasswordChange.EmptyOrWrongPassword",
            "Password is empty or wrong."
        );
    }

    public static class PasswordRestore
    {
        public static readonly Error InvalidOrExpiredRestoreCode = new(
            "PasswordRestore.InvalidOrExpiredRestoreCode",
            "Restore code is invalid or has expired."
        );
    }

    public static class Deletion
    {
        public static readonly Error NotAdminRequester = new(
            "Deletion.NotAdminRequester",
            "Only admin users can delete users other that themself."
        );

        public static readonly Error EmptyOrWrongPassword = new(
            "Deletion.WrongPassword",
            "Password is empty or wrong."
        );
    }

    public static class Deactivation
    {
        public static readonly Error AlreadyDeactivated = new(
            "Deactivation.AlreadyDeactivated",
            "The user has already been deactivated."
        );

        public static readonly Error CannotDeactivateAdmin = new(
            "Deactivation.CannotDeactivateAdmin",
            "Admin users cannot be deactivated or reactivated."
        );

        public static readonly Error NotAdminRequester = new(
            "Deactivation.NotAdminRequester",
            "Only admin users can deactivate other users."
        );
    }

    public static class Reactivation
    {
        public static readonly Error AlreadyReactivated = new(
            "Reactivation.AlreadyReactivated",
            "The user is not deactivated."
        );

        public static readonly Error CannotReactivateAdmin = new(
            "Reactivation.CannotReactivateAdmin",
            "Admin users cannot be deactivated or reactivated."
        );

        public static readonly Error NotAdminRequester = new(
            "Reactivation.NotAdminRequester",
            "Only admin users can reactivate users that were deactivated by admin."
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

        public static readonly Error EmailAlreadyInUse = new(
            "Email.EmailAlreadyInUse",
            "Specified email is already in use."
        );

        public static readonly Error EmailUnverified = new(
            "Email.EmailUnverified",
            "Can not login with unverified account."
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