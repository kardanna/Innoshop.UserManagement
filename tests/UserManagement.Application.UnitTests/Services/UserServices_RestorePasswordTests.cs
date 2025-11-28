using FluentAssertions;
using Innoshop.Contracts.UserManagement.UserRoles;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Models;
using UserManagement.Application.Repositories;
using UserManagement.Application.Services;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;

namespace UserManagement.Application.UnitTests.Services;

public class UserServices_RestorePasswordTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly IUserDeactivationRepository _userDeactivationRepositoryMock;
    private readonly IPasswordRestoreAttemptRepository _passwordRestoreAttemptRepositoryMock;
    private readonly ILoginAttemptRepository _loginRepositoryMock;
    private readonly IPasswordHasher<User> _hasherMock;
    private readonly IUserPolicy _userPolicyMock;
    private readonly IPasswordPolicy _passwordPolicyMock;

    private readonly IUserService _service;

    public UserServices_RestorePasswordTests()
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

    private static readonly PasswordRestoreAttempt attempt = new()
    {
        User = user,
        UserId = user.Id,
        IsSucceeded = false,
        SucceededAt = null
    };

    [Fact]
    public async Task UserService_ShouldReturnErrorOnRestorePassword_WhenAttemptRecordIsNotFound()
    {
        //Arrange
        var restoreCode = "code";
        var newPassword = "newPassword";
        _passwordRestoreAttemptRepositoryMock.GetAsync(restoreCode).Returns((PasswordRestoreAttempt)null!);

        //Act
        var result = await _service.RestorePasswordAsync(restoreCode, newPassword);

        //Assert
        result.Error.Should().Be(DomainErrors.PasswordRestore.InvalidOrExpiredRestoreCode);
    }

    [Fact]
    public async Task UserService_ShouldReturnErrorOnRestorePassword_WhenPolicyDenies()
    {
        //Arrange
        var restoreCode = "code";
        var newPassword = "newPassword";
        _passwordRestoreAttemptRepositoryMock.GetAsync(restoreCode).Returns(attempt);
        var error = DomainErrors.PasswordRestore.InvalidOrExpiredRestoreCode;
        _passwordPolicyMock.IsPasswordRestoreAllowed(attempt).Returns(error);

        //Act
        var result = await _service.RestorePasswordAsync(restoreCode, newPassword);

        //Assert
        result.Error.Should().Be(error);
        attempt.IsSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task UserService_ShouldReturnSuccessOnRestorePassword_WhenPolicyAllows()
    {
        //Arrange
        var restoreCode = "code";
        var newPassword = "newPassword";
        _passwordRestoreAttemptRepositoryMock.GetAsync(restoreCode).Returns(attempt);
        _passwordPolicyMock.IsPasswordRestoreAllowed(attempt).Returns(PolicyResult.Success);

        //Act
        var result = await _service.RestorePasswordAsync(restoreCode, newPassword);

        //Assert
        result.Value.Should().Be(user.Id);
        attempt.IsSucceeded.Should().BeTrue();
    }
}