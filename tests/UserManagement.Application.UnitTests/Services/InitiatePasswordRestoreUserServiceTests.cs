using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Repositories;
using UserManagement.Application.Services;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;

namespace UserManagement.Application.UnitTests.Services;

public class InitiatePasswordRestoreUserServiceTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly IUserDeactivationRepository _userDeactivationRepositoryMock;
    private readonly IPasswordRestoreAttemptRepository _passwordRestoreAttemptRepositoryMock;
    private readonly ILoginAttemptRepository _loginRepositoryMock;
    private readonly IPasswordHasher<User> _hasherMock;
    private readonly IUserPolicy _userPolicyMock;
    private readonly IPasswordPolicy _passwordPolicyMock;

    private readonly IUserService _service;

    public InitiatePasswordRestoreUserServiceTests()
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

    private static readonly User user = new(); 

    [Fact]
    public async Task UserService_ShouldReturnErrorOnInitiatePasswordRestrore_WhenUserIsNotFound()
    {
        //Arrange
        var userEmail = "user@email.com";
        _userRepositoryMock.GetAsync(userEmail).Returns((User)null!);

        //Act
        var result = await _service.InitiatePasswordRestorationAsync(userEmail);

        //Assert
        result.Error.Should().Be(DomainErrors.User.NotFound);
    }

    [Fact]
    public async Task UserService_ShouldReturnSuccessOnInitiatePasswordRestrore_WhenUserIsFound()
    {
        //Arrange
        var userEmail = "user@email.com";
        _userRepositoryMock.GetAsync(userEmail).Returns(user);

        //Act
        var result = await _service.InitiatePasswordRestorationAsync(userEmail);

        //Assert
        result.IsSuccess.Should().BeTrue();
        _passwordRestoreAttemptRepositoryMock.Received(1).RemovePreviousUnseccessfulAttempts(user.Id);
        _passwordRestoreAttemptRepositoryMock.Received(1).Add(Arg.Is<PasswordRestoreAttempt>(a => a.User == user));
    }
}