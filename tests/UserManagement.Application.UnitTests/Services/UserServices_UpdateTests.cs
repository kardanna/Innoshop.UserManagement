using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Models;
using UserManagement.Application.Repositories;
using UserManagement.Application.Services;
using UserManagement.Application.UseCases.Users.Update;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;

namespace UserManagement.Application.UnitTests.Services;

public class UserServices_UpdateTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly IUserDeactivationRepository _userDeactivationRepositoryMock;
    private readonly IPasswordRestoreAttemptRepository _passwordRestoreAttemptRepositoryMock;
    private readonly ILoginAttemptRepository _loginRepositoryMock;
    private readonly IPasswordHasher<User> _hasherMock;
    private readonly IUserPolicy _userPolicyMock;
    private readonly IPasswordPolicy _passwordPolicyMock;

    private readonly IUserService _service;

    public UserServices_UpdateTests()
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
    };

    private static readonly UpdateUserCommand command = new(
        Guid.CreateVersion7(),
        "name",
        "surname",
        DateOnly.FromDateTime(DateTime.UtcNow)
    );

    [Fact]
    public async Task UserService_ShouldReturnErrorOnUpdate_WhenUserIsNotFound()
    {
        //Arrange
        var context = new UpdateUserContext(command);
        _userRepositoryMock.GetAsync(command.UserId).Returns((User)null!);

        //Act
        var result = await _service.UpdateAsync(context);

        //Assert
        result.Error.Should().Be(DomainErrors.User.NotFound);
    }

    [Fact]
    public async Task UserService_ShouldReturnErrorOnUpdate_WhenPolicyDenies()
    {
        //Arrange
        var context = new UpdateUserContext(command);
        var error = DomainErrors.Register.IllegalAge;
        _userRepositoryMock.GetAsync(command.UserId).Returns(user);
        _userPolicyMock.IsUpdateAllowedAsync(user, context).Returns(error);

        //Act
        var result = await _service.UpdateAsync(context);

        //Assert
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task UserService_ShouldReturnErrorOnUpdate_WhenPolicyAllows()
    {
        //Arrange
        var context = new UpdateUserContext(command);
        _userRepositoryMock.GetAsync(command.UserId).Returns(user);
        _userPolicyMock.IsUpdateAllowedAsync(user, context).Returns(PolicyResult.Success);

        //Act
        var result = await _service.UpdateAsync(context);

        //Assert
        result.Value.Should().Be(user);
        result.Value.FirstName.Should().Be(command.FirstName);
        result.Value.LastName.Should().Be(command.LastName);
        result.Value.DateOfBirth.Should().Be(command.DateOfBirth);
    }
}