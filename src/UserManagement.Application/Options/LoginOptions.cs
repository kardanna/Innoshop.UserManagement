namespace UserManagement.Application.Options;

public class LoginOptions
{
    public int LoginAttemptsMaxCount { get; init; }
    public int LoginAttemptsTimeWindowInMinutes { get; init; }
}