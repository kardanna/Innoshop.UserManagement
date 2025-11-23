namespace UserManagement.Domain.Exceptions;

public class UninitializedExchangeChannelException : Exception
{
    public UninitializedExchangeChannelException()
    {
    }

    public UninitializedExchangeChannelException(string message)
        : base(message)
    {
    }
}