using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Repositories;
using UserManagement.Application.Services;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.UnitTests.Services;

public class UserService_IsDeactivatedTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly IUserDeactivationRepository _userDeactivationRepositoryMock;
    private readonly IPasswordRestoreAttemptRepository _passwordRestoreAttemptRepositoryMock;
    private readonly ILoginAttemptRepository _loginRepositoryMock;
    private readonly IPasswordHasher<User> _hasherMock;
    private readonly IUserPolicy _userPolicyMock;
    private readonly IPasswordPolicy _passwordPolicyMock;

    private readonly IUserService _service;

    public UserService_IsDeactivatedTests()
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
        Id = Guid.CreateVersion7()
    };

    [Fact]
    public async Task UserService_ShouldReturnFalseOnUserDeactivated_WhenDeactivationRecordIsNotFound()
    {
        //Arrange
        _userDeactivationRepositoryMock.GetLatestAsync(user.Id).Returns((UserDeactivation)null!);
       
        //Act
        var result = await _service.IsDeacivated(user.Id);

        //Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserService_ShouldReturnTrueOnUserDeactivated_WhenDeactivationRecordIsFoundAndReactivationIsNotSet()
    {
        //Arrange
        var record = new UserDeactivation()
        {
            UserId = user.Id,
            User = user,
            DeactivatedAt = DateTime.UtcNow.AddYears(-1),
            DeactivationRequester = user,
            DeactivationRequesterId = user.Id,
            ReactivatedAt = null,
            ReactivationRequester = null,
            ReactivationRequesterId = null
        };
        _userDeactivationRepositoryMock.GetLatestAsync(user.Id).Returns(record);

        //Act
        var result = await _service.IsDeacivated(user.Id);

        //Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserService_ShouldReturnFalseOnUserDeactivated_WhenDeactivationRecordIsFoundAndReactivationIsSet()
    {
        //Arrange
        var record = new UserDeactivation()
        {
            UserId = user.Id,
            User = user,
            DeactivatedAt = DateTime.UtcNow.AddYears(-1),
            DeactivationRequester = user,
            DeactivationRequesterId = user.Id,
            ReactivatedAt = DateTime.UtcNow,
            ReactivationRequester = user,
            ReactivationRequesterId = user.Id
        };
        _userDeactivationRepositoryMock.GetLatestAsync(user.Id).Returns(record);

        //Act
        var result = await _service.IsDeacivated(user.Id);

        //Assert
        result.Should().BeFalse();
    }
}