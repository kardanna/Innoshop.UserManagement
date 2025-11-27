using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Models;
using UserManagement.Application.Repositories;
using UserManagement.Application.Services;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;

namespace UserManagement.Application.UnitTests.Services;

public class UserService_DeactivateTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly IUserDeactivationRepository _userDeactivationRepositoryMock;
    private readonly IPasswordRestoreAttemptRepository _passwordRestoreAttemptRepositoryMock;
    private readonly ILoginAttemptRepository _loginRepositoryMock;
    private readonly IPasswordHasher<User> _hasherMock;
    private readonly IUserPolicy _userPolicyMock;
    private readonly IPasswordPolicy _passwordPolicyMock;

    private readonly IUserService _service;

    public UserService_DeactivateTests()
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

    private static readonly User subject = new()
    {
        Id = Guid.CreateVersion7(),
        Roles = [ Role.Customer ]
    };

    private static readonly User requester = new()
    {
        Id = Guid.CreateVersion7(),
        Roles = [ Role.Administrator ]
    };

    [Fact]
    public async Task UserService_ShouldReturnErrorOnDeactivate_WhenSubjectIsNotFound()
    {
        //Arrange
        _userRepositoryMock.GetAsync(subject.Id).Returns((User)null!);

        //Act
        var result = await _service.DeactivateAsync(subject.Id, requester.Id);

        //Assert
        result.Error.Should().Be(DomainErrors.User.NotFound);
        _userDeactivationRepositoryMock.Received(0).Add(Arg.Any<UserDeactivation>());
    }

    [Fact]
    public async Task UserService_ShouldReturnErrorOnDeactivate_WhenRequesterIsNotSubjectAndRequesterIsNotFound()
    {
        //Arrange
        _userRepositoryMock.GetAsync(subject.Id).Returns(subject);
        _userRepositoryMock.GetAsync(requester.Id).Returns((User)null!);

        //Act
        var result = await _service.DeactivateAsync(subject.Id, requester.Id);

        //Assert
        result.Error.Should().Be(DomainErrors.User.NotFound);
        _userDeactivationRepositoryMock.Received(0).Add(Arg.Any<UserDeactivation>());
    }

    [Fact]
    public async Task UserService_ShouldReturnErrorOnDeactivate_WhenPolicyDenies()
    {
        //Arrange
        _userRepositoryMock.GetAsync(subject.Id).Returns(subject);
        _userRepositoryMock.GetAsync(requester.Id).Returns(requester);
        var error = DomainErrors.Deactivation.AlreadyDeactivated;
        _userPolicyMock.IsDeactivationAllowedAsync(subject, requester).Returns(error);

        //Act
        var result = await _service.DeactivateAsync(subject.Id, requester.Id);

        //Assert
        result.Error.Should().Be(error);
        _userDeactivationRepositoryMock.Received(0).Add(Arg.Any<UserDeactivation>());
    }

    [Fact]
    public async Task UserService_ShouldReturnSuccessOnDeactivate_WhenPolicyAllows()
    {
        //Arrange
        _userRepositoryMock.GetAsync(subject.Id).Returns(subject);
        _userRepositoryMock.GetAsync(requester.Id).Returns(requester);
        _userPolicyMock.IsDeactivationAllowedAsync(subject, requester).Returns(PolicyResult.Success);

        //Act
        var result = await _service.DeactivateAsync(subject.Id, requester.Id);

        //Assert
        result.IsSuccess.Should().BeTrue();
        _userDeactivationRepositoryMock.Received(1).Add(Arg.Is<UserDeactivation>(
            ud => ud.User == subject && ud.DeactivationRequester == requester));
    }
}