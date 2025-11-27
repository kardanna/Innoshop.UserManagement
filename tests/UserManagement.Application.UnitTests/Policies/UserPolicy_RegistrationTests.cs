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
using UserManagement.Application.UseCases.Users.Register;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;

namespace UserManagement.Application.UnitTests.Policies;

public class UserPolicy_RegistrationTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly ILoginAttemptRepository _loginRepositoryMock;
    private readonly IUserDeactivationRepository _userDeactivationRepositoryMock;
    private readonly IOptions<RegistrationOptions> _registrationOptionsMock;
    private readonly IOptions<LoginOptions> _loginOptionsMock;
    private readonly IPasswordHasher<User> _hasherMock;

    private readonly IUserPolicy _policy;

    public UserPolicy_RegistrationTests()
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

    private static readonly RegisterUserCommand registerUserCommand = new(
        "Victor",
        "Victorov",
        DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-100)),
        "victor.victorov@gmail.com",
        "password"
    );

    [Fact]
    public async Task UserPolicy_ShouldDenyRegistration_WhenUserIsTooYoung()
    {
        //Arrange
        var command = registerUserCommand with { DateOfBirth = DateOnly.FromDateTime(DateTime.Now) };
        var context = new RegistrationContext(command, Role.Customer);

        //Act
        var result = await _policy.IsRegistrationAllowedAsync(context);

        //Assert
        result.Error.Should().Be(DomainErrors.Register.IllegalAge);
    }

    [Fact]
    public async Task UserPolicy_ShouldDenyRegistration_WhenEmailIsTaken()
    {
        //Arrange
        var context = new RegistrationContext(registerUserCommand, Role.Customer);
        _userRepositoryMock.CountUsersWithEmailAsync(registerUserCommand.Email).Returns(1);

        //Act
        var result = await _policy.IsRegistrationAllowedAsync(context);

        //Assert
        result.Error.Should().Be(DomainErrors.Email.EmailAlreadyInUse);
    }

    [Fact]
    public async Task UserPolicy_ShouldAllowRegistration_WhenAllRulesApply()
    {
        //Arrange
        var context = new RegistrationContext(registerUserCommand, Role.Customer);
        _userRepositoryMock.CountUsersWithEmailAsync(registerUserCommand.Email).Returns(0);

        //Act
        var result = await _policy.IsRegistrationAllowedAsync(context);

        //Assert
        result.Should().Be(PolicyResult.Success);
    }
}
