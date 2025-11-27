using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Models;
using UserManagement.Application.Repositories;
using UserManagement.Application.Services;
using UserManagement.Application.UseCases.Passwords.Change;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;

namespace UserManagement.Application.UnitTests.Services;

public class ChangePasswordUserServiceTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly IUserDeactivationRepository _userDeactivationRepositoryMock;
    private readonly IPasswordRestoreAttemptRepository _passwordRestoreAttemptRepositoryMock;
    private readonly ILoginAttemptRepository _loginRepositoryMock;
    private readonly IPasswordHasher<User> _hasherMock;
    private readonly IUserPolicy _userPolicyMock;
    private readonly IPasswordPolicy _passwordPolicyMock;

    private readonly IUserService _service;

    public ChangePasswordUserServiceTests()
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
        Roles = [ Role.Customer ]
    };

    private static readonly ChangePasswordCommand command = new(
        Guid.CreateVersion7(),
        "oldPassword",
        "newPassword"
    );

    [Fact]
    public async Task UserService_ShouldReturnErrorOnChangePassword_WhenUserIsNotFound()
    {
        //Arrange
        var context = new ChangePasswordContext(command with { UserId = user.Id });
        _userRepositoryMock.GetAsync(context.UserId).Returns((User)null!);

        //Act
        var result = await _service.ChangePasswordAsync(context);

        //Assert
        result.Error.Should().Be(DomainErrors.User.NotFound);
    }

    [Fact]
    public async Task UserService_ShouldReturnErrorOnChangePassword_WhenPolicyDenies()
    {
        //Arrange
        var context = new ChangePasswordContext(command with { UserId = user.Id });
        _userRepositoryMock.GetAsync(context.UserId).Returns(user);
        var error = DomainErrors.PasswordChange.EmptyOrWrongPassword;
        _passwordPolicyMock.IsPasswordChangeAllowedAsync(user, context).Returns(error);

        //Act
        var result = await _service.ChangePasswordAsync(context);

        //Assert
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task UserService_ShouldReturnSuccessOnChangePassword_WhenPolicyAllows()
    {
        //Arrange
        var context = new ChangePasswordContext(command with { UserId = user.Id });
        _userRepositoryMock.GetAsync(context.UserId).Returns(user);
        _passwordPolicyMock.IsPasswordChangeAllowedAsync(user, context).Returns(PolicyResult.Success);

        //Act
        var result = await _service.ChangePasswordAsync(context);

        //Assert
        result.IsSuccess.Should().BeTrue();
    }
}