using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NSubstitute;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Models;
using UserManagement.Application.Options;
using UserManagement.Application.Policies;
using UserManagement.Application.Repositories;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;

namespace UserManagement.Application.UnitTests.Policies;

public class ReactivateUserPolicyTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly ILoginAttemptRepository _loginRepositoryMock;
    private readonly IUserDeactivationRepository _userDeactivationRepositoryMock;
    private readonly IOptions<RegistrationOptions> _registrationOptionsMock;
    private readonly IOptions<LoginOptions> _loginOptionsMock;
    private readonly IPasswordHasher<User> _hasherMock;

    private readonly IUserPolicy _policy;

    public ReactivateUserPolicyTests()
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
        PasswordHash = "hash",
        Roles = [ Role.Customer ],
        IsDeleted = false
    };

    private static readonly User requester = new()
    {
        Id = Guid.CreateVersion7(),
        IsEmailVerified = true,
        PasswordHash = "hash",
        Roles = [ Role.Administrator ],
        IsDeleted = false
    };

    [Fact]
    public async Task UserPolicy_ShouldDenyReactivation_WhenSubjectUserIsDeleted()
    {
        //Arrange
        user.IsDeleted = true;
        requester.IsDeleted = false;

        //Act
        var result = await _policy.IsReactivationAllowedAsync(user, requester);

        //Assert
        result.Error.Should().Be(DomainErrors.User.NotFound);
    }

    [Fact]
    public async Task UserPolicy_ShouldDenyReactivation_WhenRequesterIsDeleted()
    {
        //Arrange
        user.IsDeleted = false;
        requester.IsDeleted = true;

        //Act
        var result = await _policy.IsReactivationAllowedAsync(user, requester);

        //Assert
        result.Error.Should().Be(DomainErrors.User.NotFound);
    }

    [Fact]
    public async Task UserPolicy_ShouldDenyReactivation_WhenSubjectUserIsAdmin()
    {
        //Arrange
        user.IsDeleted = false;
        requester.IsDeleted = false;
        user.Roles = [ Role.Administrator ];

        //Act
        var result = await _policy.IsReactivationAllowedAsync(user, user);

        //Assert
        result.Error.Should().Be(DomainErrors.Reactivation.CannotReactivateAdmin);
    }
    
    [Fact]
    public async Task UserPolicy_ShouldDenyReactivation_WhenSubjectIsAlreadyReactivated()
    {
        //Arrange
        user.IsDeleted = false;
        requester.IsDeleted = false;
        user.Roles = [ Role.Customer ];
        _userDeactivationRepositoryMock.GetLatestAsync(user.Id).Returns((UserDeactivation)null!);

        //Act
        var result = await _policy.IsReactivationAllowedAsync(user, user);

        //Assert
        result.Error.Should().Be(DomainErrors.Reactivation.AlreadyReactivated);
    }

    [Fact]
    public async Task UserPolicy_ShouldDenyReactivation_WhenRequesterIsSubjectAndSubjectIsNotAdminAndSubjectWasDeactivatedByAdmin()
    {
        //Arrange
        user.IsDeleted = false;
        requester.IsDeleted = false;
        user.Roles = [ Role.Customer ];
        requester.Roles = [ Role.Administrator ];
        _userDeactivationRepositoryMock.GetLatestAsync(user.Id).Returns(new UserDeactivation()
            {
                User = user,
                DeactivationRequester = requester
            }
        );

        //Act
        var result = await _policy.IsReactivationAllowedAsync(user, user);

        //Assert
        result.Error.Should().Be(DomainErrors.Reactivation.NotAuthorized);
    }

    [Fact]
    public async Task UserPolicy_ShouldDenyReactivation_WhenRequesterIsNotSubjectAndRequesterIsNotAdmin()
    {
        //Arrange
        user.IsDeleted = false;
        requester.IsDeleted = false;
        user.Roles = [ Role.Customer ];
        requester.Roles = [ Role.Customer ];
        _userDeactivationRepositoryMock.GetLatestAsync(user.Id).Returns(new UserDeactivation()
            {
                User = user,
                DeactivationRequester = user
            }
        );

        //Act
        var result = await _policy.IsReactivationAllowedAsync(user, requester);

        //Assert
        result.Error.Should().Be(DomainErrors.Reactivation.NotAuthorized);
    }

    [Fact]
    public async Task UserPolicy_ShouldAllowReactivation_WhenRequesterIsSubjectAndSubjectWasNotDeactivatedByAdmin()
    {
        //Arrange
        user.IsDeleted = false;
        requester.IsDeleted = false;
        user.Roles = [ Role.Customer ];
        requester.Roles = [ Role.Customer ];
        _userDeactivationRepositoryMock.GetLatestAsync(user.Id).Returns(new UserDeactivation()
            {
                User = user,
                DeactivationRequester = user
            }
        );

        //Act
        var result = await _policy.IsReactivationAllowedAsync(user, user);

        //Assert
        result.Should().Be(PolicyResult.Success);
    }

    [Fact]
    public async Task UserPolicy_ShouldAllowReactivation_WhenRequesterIsNotSubjectAndRequesterIsAdmin()
    {
        //Arrange
        user.IsDeleted = false;
        requester.IsDeleted = false;
        user.Roles = [ Role.Customer ];
        requester.Roles = [ Role.Administrator ];
        _userDeactivationRepositoryMock.GetLatestAsync(user.Id).Returns(new UserDeactivation()
            {
                User = user,
                DeactivationRequester = requester
            }
        );

        //Act
        var result = await _policy.IsReactivationAllowedAsync(user, requester);

        //Assert
        result.Should().Be(PolicyResult.Success);
    }
}
