using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Models;
using UserManagement.Application.Repositories;
using UserManagement.Application.Services;
using UserManagement.Application.UseCases.Users.Login;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;

namespace UserManagement.Application.UnitTests.Services;

public class UserService_LoginTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly IUserDeactivationRepository _userDeactivationRepositoryMock;
    private readonly IPasswordRestoreAttemptRepository _passwordRestoreAttemptRepositoryMock;
    private readonly ILoginAttemptRepository _loginRepositoryMock;
    private readonly IPasswordHasher<User> _hasherMock;
    private readonly IUserPolicy _userPolicyMock;
    private readonly IPasswordPolicy _passwordPolicyMock;

    private readonly IUserService _service;

    public UserService_LoginTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _userDeactivationRepositoryMock = Substitute.For<IUserDeactivationRepository>();
        _passwordRestoreAttemptRepositoryMock = Substitute.For<IPasswordRestoreAttemptRepository>();
        _loginRepositoryMock = Substitute.For<ILoginAttemptRepository>();
        _hasherMock = Substitute.For<IPasswordHasher<User>>();
        _userPolicyMock = Substitute.For<IUserPolicy>();
        _passwordPolicyMock = Substitute.For<IPasswordPolicy>();

        _service = new UserService(
            _userRepositoryMock,
            _userDeactivationRepositoryMock,
            _passwordRestoreAttemptRepositoryMock,
            _loginRepositoryMock,
            _hasherMock,
            _userPolicyMock,
            _passwordPolicyMock
        );
    }

    private static readonly User user = new()
    {
        Id = Guid.CreateVersion7(),
        Email = "user@email.com",
        PasswordHash = "hash"
    };

    private static readonly LoginUserCommand command = new(
        "user@email.com",
        "password",
        "deviceFingerprint"
    );

    [Fact]
    public async Task UserService_ShouldReturnErrorOnLogin_WhenUserEmailIsNotFound()
    {
        //Arrange
        var context = new LoginUserContext(command);
        _userRepositoryMock.GetAsync(context.Email).Returns((User)null!);

        //Act
        var result = await _service.LoginAsync(context);

        //Assert
        result.Error.Should().Be(DomainErrors.Login.WrongEmailOrPassword);
    }

    [Fact]
    public async Task UserService_ShouldReturnErrorOnLogin_WhenPolicyDeniesLogin()
    {
        //Arrange
        var context = new LoginUserContext(command);
        _userRepositoryMock.GetAsync(context.Email).Returns(user);
        var error = DomainErrors.Login.WrongEmailOrPassword;
        _userPolicyMock.IsLoginAllowedAsync(user, context).Returns(error);

        //Act
        var result = await _service.LoginAsync(context);

        //Assert
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task UserService_ShouldReturnUserOnLogin_WhenUserIsRegisteredAndPolicyAllows()
    {
        //Arrange
        var context = new LoginUserContext(command);
        _userRepositoryMock.GetAsync(context.Email).Returns(user);
        _userPolicyMock.IsLoginAllowedAsync(user, context).Returns(PolicyResult.Success);

        //Act
        var result = await _service.LoginAsync(context);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(user);
    }
}