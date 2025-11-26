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
using UserManagement.Application.UseCases.Users.Update;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;

namespace UserManagement.Application.UnitTests.Policies;

public class UpdateUserPolicyTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly ILoginAttemptRepository _loginRepositoryMock;
    private readonly IUserDeactivationRepository _userDeactivationRepositoryMock;
    private readonly IOptions<RegistrationOptions> _registrationOptionsMock;
    private readonly IOptions<LoginOptions> _loginOptionsMock;
    private readonly IPasswordHasher<User> _hasherMock;

    private readonly IUserPolicy _policy;

    public UpdateUserPolicyTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _loginRepositoryMock = Substitute.For<ILoginAttemptRepository>();
        _userDeactivationRepositoryMock = Substitute.For<IUserDeactivationRepository>();
        _registrationOptionsMock = Substitute.For<IOptions<RegistrationOptions>>();
        _loginOptionsMock = Substitute.For<IOptions<LoginOptions>>();
        _hasherMock = Substitute.For<IPasswordHasher<User>>();

        var registrationOptions = new RegistrationOptions() { MustBeAtLeastYears = 14 };
        _registrationOptionsMock.Value.Returns(registrationOptions);

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
        DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-100)),
        IsEmailVerified = true,
        IsDeleted = false
    };

    private static readonly UpdateUserCommand updateUserCommand = new(
        Guid.CreateVersion7(),
        "Victor",
        "Victorov",
        DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-100))
    );

    [Fact]
    public async Task UserPolicy_ShouldDenyUpdate_WhenUserEmailIsUnverified()
    {
        //Arrange
        user.IsEmailVerified = false;
        var context = new UpdateUserContext(updateUserCommand);

        //Act
        var result = await _policy.IsUpdateAllowedAsync(user, context);
        user.IsEmailVerified = true;

        //Assert
        result.Error.Should().Be(DomainErrors.Email.EmailUnverified);
    }

    [Fact]
    public async Task UserPolicy_ShouldDenyUpdate_WhenUserIsDeactivated()
    {
        //Arrange
        user.IsEmailVerified = true;
        var context = new UpdateUserContext(updateUserCommand);
        _userDeactivationRepositoryMock.GetLatestAsync(user.Id).Returns(new UserDeactivation());

        //Act
        var result = await _policy.IsUpdateAllowedAsync(user, context);

        //Assert
        result.Error.Should().Be(DomainErrors.User.Deactivated);
    }

    [Fact]
    public async Task UserPolicy_ShouldDenyUpdate_WhenUserIsTooYoung()
    {
        //Arrange
        var command = updateUserCommand with { DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow) };
        var context = new UpdateUserContext(command);
        _userDeactivationRepositoryMock.GetLatestAsync(user.Id).Returns((UserDeactivation)null!);

        //Act
        var result = await _policy.IsUpdateAllowedAsync(user, context);

        //Assert
        result.Error.Should().Be(DomainErrors.Register.IllegalAge);
    }

    [Fact]
    public async Task UserPolicy_ShouldAllowUpdate_WhenAllRulesApply()
    {
        //Arrange
        var context = new UpdateUserContext(updateUserCommand);
        _userDeactivationRepositoryMock.GetLatestAsync(user.Id).Returns((UserDeactivation)null!);

        //Act
        var result = await _policy.IsUpdateAllowedAsync(user, context);

        //Assert
        result.Should().Be(PolicyResult.Success);
    }
}
