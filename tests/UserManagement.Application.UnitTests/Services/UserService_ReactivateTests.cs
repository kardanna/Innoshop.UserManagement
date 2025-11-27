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

public class UserService_ReactivateTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly IUserDeactivationRepository _userDeactivationRepositoryMock;
    private readonly IPasswordRestoreAttemptRepository _passwordRestoreAttemptRepositoryMock;
    private readonly ILoginAttemptRepository _loginRepositoryMock;
    private readonly IPasswordHasher<User> _hasherMock;
    private readonly IUserPolicy _userPolicyMock;
    private readonly IPasswordPolicy _passwordPolicyMock;

    private readonly IUserService _service;

    public UserService_ReactivateTests()
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

    private static readonly UserDeactivation record = new();

    [Fact]
    public async Task UserService_ShouldReturnErrorOnReactivate_WhenSubjectIsNotFound()
    {
        //Arrange
        _userRepositoryMock.GetAsync(subject.Id).Returns((User)null!);

        //Act
        var result = await _service.ReactivateAsync(subject.Id, requester.Id);

        //Assert
        result.Error.Should().Be(DomainErrors.User.NotFound);
    }

    [Fact]
    public async Task UserService_ShouldReturnErrorOnReactivate_WhenRequesterIsNotSubjectAndRequesterIsNotFound()
    {
        //Arrange
        _userRepositoryMock.GetAsync(subject.Id).Returns(subject);
        _userRepositoryMock.GetAsync(requester.Id).Returns((User)null!);

        //Act
        var result = await _service.ReactivateAsync(subject.Id, requester.Id);

        //Assert
        result.Error.Should().Be(DomainErrors.User.NotFound);
    }

    [Fact]
    public async Task UserService_ShouldReturnErrorOnReactivate_WhenDeactivationRecordIsNotFound()
    {
        //Arrange
        _userRepositoryMock.GetAsync(subject.Id).Returns(subject);
        _userRepositoryMock.GetAsync(requester.Id).Returns(requester);
        _userDeactivationRepositoryMock.GetLatestAsync(subject.Id).Returns((UserDeactivation)null!);
        
        //Act
        var result = await _service.ReactivateAsync(subject.Id, requester.Id);

        //Assert
        result.Error.Should().Be(DomainErrors.Reactivation.AlreadyReactivated);
    }

    [Fact]
    public async Task UserService_ShouldReturnErrorOnReactivate_WhenPolicyDenies()
    {
        //Arrange
        _userRepositoryMock.GetAsync(subject.Id).Returns(subject);
        _userRepositoryMock.GetAsync(requester.Id).Returns(requester);
        _userDeactivationRepositoryMock.GetLatestAsync(subject.Id).Returns(record);
        var error = DomainErrors.Reactivation.AlreadyReactivated;
        _userPolicyMock.IsReactivationAllowedAsync(subject, requester, record).Returns(error);

        //Act
        var result = await _service.ReactivateAsync(subject.Id, requester.Id);

        //Assert
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task UserService_ShouldReturnSuccessOnReactivate_WhenPolicyAllows()
    {
        //Arrange
        _userRepositoryMock.GetAsync(subject.Id).Returns(subject);
        _userRepositoryMock.GetAsync(requester.Id).Returns(requester);
        _userDeactivationRepositoryMock.GetLatestAsync(subject.Id).Returns(record);
        _userPolicyMock.IsReactivationAllowedAsync(subject, requester, record).Returns(PolicyResult.Success);

        //Act
        var result = await _service.ReactivateAsync(subject.Id, requester.Id);

        //Assert
        result.IsSuccess.Should().BeTrue();
        record.ReactivationRequester.Should().Be(requester);
    }
}