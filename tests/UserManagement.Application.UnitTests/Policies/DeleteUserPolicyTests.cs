using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NSubstitute;
using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Models;
using UserManagement.Application.Options;
using UserManagement.Application.Policies;
using UserManagement.Application.Repositories;
using UserManagement.Application.UseCases.Users.Delete;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;

namespace UserManagement.Application.UnitTests.Policies;

public class DeleteUserPolicyTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly ILoginAttemptRepository _loginRepositoryMock;
    private readonly IUserDeactivationRepository _userDeactivationRepositoryMock;
    private readonly IOptions<RegistrationOptions> _registrationOptionsMock;
    private readonly IOptions<LoginOptions> _loginOptionsMock;
    private readonly IPasswordHasher<User> _hasherMock;

    private readonly IUserPolicy _policy;

    public DeleteUserPolicyTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _loginRepositoryMock = Substitute.For<ILoginAttemptRepository>();
        _userDeactivationRepositoryMock = Substitute.For<IUserDeactivationRepository>();
        _registrationOptionsMock = Substitute.For<IOptions<RegistrationOptions>>();
        _loginOptionsMock = Substitute.For<IOptions<LoginOptions>>();
        _hasherMock = Substitute.For<IPasswordHasher<User>>();

        _policy = new UserPolicy(
            _userRepositoryMock,
            _loginRepositoryMock,
            _userDeactivationRepositoryMock,
            _registrationOptionsMock,
            _loginOptionsMock,
            _hasherMock
        );
    }

    private static readonly User user = new()
    {
        Id = Guid.CreateVersion7(),
        IsEmailVerified = true,
        Roles = [ Role.Customer ],
        IsDeleted = false
    };

    private static readonly User requester = new()
    {
        Id = Guid.CreateVersion7(),
        IsEmailVerified = true,
        Roles = [ Role.Administrator ],
        IsDeleted = false
    };

    private static readonly DeleteUserCommand deleteUserCommand = new(
        Guid.CreateVersion7(),
        "password",
        Guid.CreateVersion7()
    );

    [Fact]
    public async Task UserPolicy_ShouldDenyDeletion_WhenSubjectUserIsDeleted()
    {
        //Arrange
        user.IsDeleted = true;
        requester.IsDeleted = false;
        var context = new DeleteUserContext(deleteUserCommand);

        //Act
        var result = await _policy.IsDeletionAllowedAsync(user, requester, context);

        //Assert
        result.Error.Should().Be(DomainErrors.User.NotFound);
    }

    [Fact]
    public async Task UserPolicy_ShouldDenyDeletion_WhenRequesterIsDeleted()
    {
        //Arrange
        user.IsDeleted = false;
        requester.IsDeleted = true;
        var context = new DeleteUserContext(deleteUserCommand);

        //Act
        var result = await _policy.IsDeletionAllowedAsync(user, requester, context);

        //Assert
        result.Error.Should().Be(DomainErrors.User.NotFound);
    }

    [Fact]
    public async Task UserPolicy_ShouldAllowDeletion_WhenRequesterIsAdminAndRequesterIsNotSubject()
    {
        //Arrange
        user.IsDeleted = false;
        requester.IsDeleted = false;
        requester.Roles = [ Role.Administrator ];
        var context = new DeleteUserContext(deleteUserCommand);

        //Act
        var result = await _policy.IsDeletionAllowedAsync(user, requester, context);

        //Assert
        result.Should().Be(PolicyResult.Success);
    }

    [Fact]
    public async Task UserPolicy_ShouldDenyDeletion_WhenRequesterIsNotSubjectAndRequesterIsNotAdmin()
    {
        //Arrange
        user.IsDeleted = false;
        requester.IsDeleted = false;
        requester.Roles = [ Role.Customer ];
        var context = new DeleteUserContext(deleteUserCommand);

        //Act
        var result = await _policy.IsDeletionAllowedAsync(user, requester, context);

        //Assert
        result.Error.Should().Be(DomainErrors.Deletion.NotAdminRequester);
    }

    [Fact]
    public async Task UserPolicy_ShouldDenyDeletion_WhenUserPasswordIsEmpty()
    {
        //Arrange
        user.IsDeleted = false;
        requester.IsDeleted = false;
        var command = deleteUserCommand with { Password = "" };
        var context = new DeleteUserContext(command);

        //Act
        var result = await _policy.IsDeletionAllowedAsync(user, user, context);

        //Assert
        result.Error.Should().Be(DomainErrors.Deletion.EmptyOrWrongPassword);
    }

    [Fact]
    public async Task UserPolicy_ShouldDenyDeletion_WhenUserPasswordIsWhiteSpaces()
    {
        //Arrange
        user.IsDeleted = false;
        requester.IsDeleted = false;
        var command = deleteUserCommand with { Password = "             " };
        var context = new DeleteUserContext(command);

        //Act
        var result = await _policy.IsDeletionAllowedAsync(user, user, context);

        //Assert
        result.Error.Should().Be(DomainErrors.Deletion.EmptyOrWrongPassword);
    }
    
    [Fact]
    public async Task UserPolicy_ShouldDenyDeletion_WhenUserPasswordIsWrong()
    {
        //Arrange
        user.IsDeleted = false;
        requester.IsDeleted = false;
        var context = new DeleteUserContext(deleteUserCommand);
        _hasherMock.VerifyHashedPassword(null!, Arg.Any<string>(), Arg.Any<string>()).Returns(PasswordVerificationResult.Failed);

        //Act
        var result = await _policy.IsDeletionAllowedAsync(user, user, context);

        //Assert
        result.Error.Should().Be(DomainErrors.Deletion.EmptyOrWrongPassword);
    }

    [Fact]
    public async Task UserPolicy_ShouldAllowDeletion_WhenRequesterIsSubjectAndAllRulesApply()
    {
        //Arrange
        user.IsDeleted = false;
        requester.IsDeleted = false;
        var context = new DeleteUserContext(deleteUserCommand);
        _hasherMock.VerifyHashedPassword(null!, user.PasswordHash, context.Password).Returns(PasswordVerificationResult.Success);

        //Act
        var result = await _policy.IsDeletionAllowedAsync(user, user, context);

        //Assert
        result.Should().Be(PolicyResult.Success);
    }
}