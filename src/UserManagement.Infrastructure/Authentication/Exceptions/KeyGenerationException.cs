namespace UserManagement.Infrastructure.Authentication.Exceptions;

public class KeyGenerationException : Exception
{
    public KeyGenerationException(string message)
        : base(message)
    {
    }
}