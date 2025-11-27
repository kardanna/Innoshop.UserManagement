using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Models;
using UserManagement.Application.Repositories;
using UserManagement.Application.Services;
using UserManagement.Application.UseCases.Users.Register;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;

namespace UserManagement.Application.UnitTests.Services;

public class UserServices_RegisterTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly IUserDeactivationRepository _userDeactivationRepositoryMock;
    private readonly IPasswordRestoreAttemptRepository _passwordRestoreAttemptRepositoryMock;
    private readonly ILoginAttemptRepository _loginRepositoryMock;
    private readonly IPasswordHasher<User> _hasherMock;
    private readonly IUserPolicy _userPolicyMock;
    private readonly IPasswordPolicy _passwordPolicyMock;

    private readonly IUserService _service;

    public UserServices_RegisterTests()
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

    private static readonly RegisterUserCommand command = new(
        "name",
        "surname",
        DateOnly.FromDateTime(DateTime.UtcNow),
        "email",
        "password"
    );

    [Fact]
    public async Task UserService_ShouldReturnErrorOnRegister_WhenPolicyDenies()
    {
        //Arrange
        var context = new RegistrationContext(command, Role.Customer);
        var error = DomainErrors.Register.IllegalAge;
        _userPolicyMock.IsRegistrationAllowedAsync(context).Returns(error);

        //Act
        var result = await _service.RegisterAsync(context);

        //Assert
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task UserService_ShouldReturnUserOnRegister_WhenPolicyAllows()
    {
        //Arrange
        var context = new RegistrationContext(command, Role.Customer);
        _userPolicyMock.IsRegistrationAllowedAsync(context).Returns(PolicyResult.Success);

        //Act
        var result = await _service.RegisterAsync(context);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be(command.Email);
        result.Value.FirstName.Should().Be(command.FirstName);
        result.Value.LastName.Should().Be(command.LastName);
        result.Value.DateOfBirth.Should().Be(command.DateOfBirth);
        _userRepositoryMock.Received(1).Add(Arg.Is<User>(u => u.Email == command.Email));
    }

    [Fact]
    public async Task UserService_ShouldCallRepositoryOnRegister_WhenPolicyAllows()
    {
        //Arrange
        var context = new RegistrationContext(command, Role.Customer);
        _userPolicyMock.IsRegistrationAllowedAsync(context).Returns(PolicyResult.Success);

        //Act
        var result = await _service.RegisterAsync(context);

        //Assert
        _userRepositoryMock.Received(1).Add(Arg.Is<User>(u => u.Email == command.Email));
    }
}