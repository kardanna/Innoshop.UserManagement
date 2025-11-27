using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NSubstitute.ClearExtensions;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Repositories;
using UserManagement.Application.Services;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;

namespace UserManagement.Application.UnitTests.Services;

public class UserService_GetTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly IUserDeactivationRepository _userDeactivationRepositoryMock;
    private readonly IPasswordRestoreAttemptRepository _passwordRestoreAttemptRepositoryMock;
    private readonly ILoginAttemptRepository _loginRepositoryMock;
    private readonly IPasswordHasher<User> _hasherMock;
    private readonly IUserPolicy _userPolicyMock;
    private readonly IPasswordPolicy _passwordPolicyMock;

    private readonly IUserService _service;

    public UserService_GetTests()
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
    };

    [Fact]
    public async Task UserService_ShouldReturnErrorOnGet_WhenUserWithIdIsNotFound()
    {
        //Arrange
        var guid = Guid.CreateVersion7();
        _userRepositoryMock.GetAsync(guid).Returns((User)null!);

        //Act
        var result = await _service.GetAsync(guid);

        //Assert
        result.Error.Should().Be(DomainErrors.User.NotFound);
    }

    [Fact]
    public async Task UserService_ShouldReturnErrorOnGet_WhenUserWithEmailIsNotFound()
    {
        //Arrange
        var email = "email";
        _userRepositoryMock.GetAsync(email).Returns((User)null!);

        //Act
        var result = await _service.GetAsync(email);

        //Assert
        result.Error.Should().Be(DomainErrors.User.NotFound);
    }

    [Fact]
    public async Task UserService_ShouldReturnUserOnGet_WhenUserWithIdIsFound()
    {
        //Arrange
        _userRepositoryMock.GetAsync(user.Id).Returns(user);

        //Act
        var result = await _service.GetAsync(user.Id);

        //Assert
        result.Value.Should().Be(user);
    }

    [Fact]
    public async Task UserService_ShouldReturnUserOnGet_WhenUserWithEmailIsFound()
    {
        //Arrange
        _userRepositoryMock.GetAsync(user.Email).Returns(user);

        //Act
        var result = await _service.GetAsync(user.Email);

        //Assert
        result.Value.Should().Be(user);
    }

    [Fact]
    public async Task UserService_ShouldCallRepositoryOnGet_WhenParameterIsGuid()
    {
        //Arrange
        var guid = Guid.CreateVersion7();
        _userRepositoryMock.ClearSubstitute();

        //Act
        await _service.GetAsync(guid);

        //Assert
        await _userRepositoryMock.Received(1).GetAsync(guid);
    }

    [Fact]
    public async Task UserService_ShouldCallRepositoryOnGet_WhenParameterIsEmail()
    {
        //Arrange
        var email = "email";
        _userRepositoryMock.ClearSubstitute();

        //Act
        await _service.GetAsync(email);

        //Assert
        await _userRepositoryMock.Received(1).GetAsync(email);
    }
}