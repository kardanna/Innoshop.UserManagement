using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Models;
using UserManagement.Application.Repositories;
using UserManagement.Application.Services;
using UserManagement.Application.UseCases.Users.Delete;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;

namespace UserManagement.Application.UnitTests.Services;

public class DeleteUserServiceTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly IUserDeactivationRepository _userDeactivationRepositoryMock;
    private readonly IPasswordRestoreAttemptRepository _passwordRestoreAttemptRepositoryMock;
    private readonly ILoginAttemptRepository _loginRepositoryMock;
    private readonly IPasswordHasher<User> _hasherMock;
    private readonly IUserPolicy _userPolicyMock;
    private readonly IPasswordPolicy _passwordPolicyMock;

    private readonly IUserService _service;

    public DeleteUserServiceTests()
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

    private static readonly DeleteUserCommand command = new(
        Guid.CreateVersion7(),
        "password",
        Guid.CreateVersion7()
    );

    [Fact]
    public async Task UserService_ShouldReturnErrorOnDelete_WhenSubjectIsNotFound()
    {
        //Arrange
        var context = new DeleteUserContext(
            command with { SubjectId = subject.Id, RequesterId = requester.Id });
        _userRepositoryMock.GetAsync(context.SubjectId).Returns((User)null!);

        //Act
        var result = await _service.DeleteAsync(context);

        //Assert
        result.Error.Should().Be(DomainErrors.User.NotFound);
    }

    [Fact]
    public async Task UserService_ShouldReturnErrorOnDelete_WhenRequesterIsNotSubjectAndRequesterIsNotFound()
    {
        //Arrange
        var context = new DeleteUserContext(
            command with { SubjectId = subject.Id, RequesterId = requester.Id });
        _userRepositoryMock.GetAsync(context.SubjectId).Returns(subject);
        _userRepositoryMock.GetAsync(context.RequesterId).Returns((User)null!);

        //Act
        var result = await _service.DeleteAsync(context);

        //Assert
        result.Error.Should().Be(DomainErrors.User.NotFound);
    }

    [Fact]
    public async Task UserService_ShouldReturnErrorOnDelete_WhenPolicyDenies()
    {
        //Arrange
        var context = new DeleteUserContext(
            command with { SubjectId = subject.Id, RequesterId = requester.Id });
        _userRepositoryMock.GetAsync(context.SubjectId).Returns(subject);
        _userRepositoryMock.GetAsync(context.RequesterId).Returns(requester);
        var error = DomainErrors.Deletion.EmptyOrWrongPassword;
        _userPolicyMock.IsDeletionAllowedAsync(subject, requester, context).Returns(error);

        //Act
        var result = await _service.DeleteAsync(context);

        //Assert
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task UserService_ShouldReturnSuccessOnDelete_WhenPolicyAllows()
    {
        //Arrange
        var context = new DeleteUserContext(
            command with { SubjectId = subject.Id, RequesterId = requester.Id });
        _userRepositoryMock.GetAsync(context.SubjectId).Returns(subject);
        _userRepositoryMock.GetAsync(context.RequesterId).Returns(requester);
        _userPolicyMock.IsDeletionAllowedAsync(subject, requester, context).Returns(PolicyResult.Success);

        //Act
        var result = await _service.DeleteAsync(context);

        //Assert
        result.IsSuccess.Should().BeTrue();
    }
}