using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NSubstitute;
using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Models;
using UserManagement.Application.Options;
using UserManagement.Application.Policies;
using UserManagement.Application.Repositories;
using UserManagement.Application.UseCases.Users.Login;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;

namespace UserManagement.Application.UnitTests.Policies;

public class LoginUserPolicyTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly ILoginAttemptRepository _loginRepositoryMock;
    private readonly IUserDeactivationRepository _userDeactivationRepositoryMock;
    private readonly IOptions<RegistrationOptions> _registrationOptionsMock;
    private readonly IOptions<LoginOptions> _loginOptionsMock;
    private readonly IPasswordHasher<User> _hasherMock;

    private readonly IUserPolicy _policy;

    public LoginUserPolicyTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _loginRepositoryMock = Substitute.For<ILoginAttemptRepository>();
        _userDeactivationRepositoryMock = Substitute.For<IUserDeactivationRepository>();
        _registrationOptionsMock = Substitute.For<IOptions<RegistrationOptions>>();
        _loginOptionsMock = Substitute.For<IOptions<LoginOptions>>();
        _hasherMock = Substitute.For<IPasswordHasher<User>>();

        var loginOptions = new LoginOptions() { LoginAttemptsMaxCount = 2, LoginAttemptsTimeWindowInMinutes = 1 };
        _loginOptionsMock.Value.Returns(loginOptions);

        _policy = new UserPolicy(
            _userRepositoryMock,
            _loginRepositoryMock,
            _userDeactivationRepositoryMock,
            _registrationOptionsMock,
            _loginOptionsMock,
            _hasherMock
        );
    }

    private static readonly User user = new()
    {
        Id = Guid.CreateVersion7(),
        IsEmailVerified = true,
        PasswordHash = "hash",
        IsDeleted = false
    };

    private static readonly LoginUserCommand loginUserCommand = new(
        "victor.victorov@gmail.com",
        "password",
        "device_fingerpring"
    );

    [Fact]
    public async Task UserPolicy_ShouldDenyLogin_WhenUserIsUnverified()
    {
        //Arrange
        user.IsEmailVerified = false;
        var context = new LoginUserContext(loginUserCommand);

        //Act
        var result = await _policy.IsLoginAllowedAsync(user, context);

        //Assert
        result.Error.Should().Be(DomainErrors.Email.EmailUnverified);
    }

    [Fact]
    public async Task UserPolicy_ShouldDenyLogin_WhenUserProvidedWrongPassword()
    {
        //Arrange
        user.IsEmailVerified = true;
        var context = new LoginUserContext(loginUserCommand);
        _hasherMock.VerifyHashedPassword(null!, user.PasswordHash, context.Password).Returns(PasswordVerificationResult.Failed);

        //Act
        var result = await _policy.IsLoginAllowedAsync(user, context);

        //Assert
        result.Error.Should().Be(DomainErrors.Login.WrongEmailOrPassword);
    }

    [Fact]
    public async Task UserPolicy_ShouldDenyLogin_WhenUserAttemptsTooManyTimes()
    {
        //Arrange
        var context = new LoginUserContext(loginUserCommand);
        user.IsEmailVerified = true;
        _hasherMock.VerifyHashedPassword(null!, user.PasswordHash, context.Password).Returns(PasswordVerificationResult.Success);
        _loginRepositoryMock.CountLoginAttemptsAsync(Arg.Any<string>(), Arg.Any<int>()).Returns(int.MaxValue);

        //Act
        var result = await _policy.IsLoginAllowedAsync(user, context);

        //Assert
        result.Error.Should().Be(DomainErrors.Login.TooManyAttempts);
    }

    [Fact]
    public async Task UserPolicy_ShouldAllowLogin_WhenAllRulesApply()
    {
        //Arrange
        var context = new LoginUserContext(loginUserCommand);
        user.IsEmailVerified = true;
        _hasherMock.VerifyHashedPassword(null!, user.PasswordHash, context.Password).Returns(PasswordVerificationResult.Success);
        _loginRepositoryMock.CountLoginAttemptsAsync(Arg.Any<string>(), Arg.Any<int>()).Returns(int.MinValue);

        //Act
        var result = await _policy.IsLoginAllowedAsync(user, context);

        //Assert
        result.Should().Be(PolicyResult.Success);
    }
}
