using FluentAssertions;
using Innoshop.Contracts.UserManagement.UserRoles;
using Microsoft.Extensions.Options;
using NSubstitute;
using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Models;
using UserManagement.Application.Options;
using UserManagement.Application.Policies;
using UserManagement.Application.Repositories;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;

namespace UserManagement.Application.UnitTests.Policies;

public class EmailPolicy_ChangeTests
{
    private readonly IOptions<EmailOptions> _optionsMock;
    private readonly IUserRepository _userRepositoryMock;
    private readonly IEmailVerificationAttemptRepository _emailVerificationAttemptRepositoryMock;
    private readonly IUserDeactivationRepository _userDeactivationRepositoryMock;

    private readonly IEmailPolicy _policy;

    public EmailPolicy_ChangeTests()
    {
        _optionsMock = Substitute.For<IOptions<EmailOptions>>();
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _emailVerificationAttemptRepositoryMock = Substitute.For<IEmailVerificationAttemptRepository>();
        _userDeactivationRepositoryMock = Substitute.For<IUserDeactivationRepository>();

        var emailOptions = new EmailOptions()
        { 
            VerificationCodeLifetimeInHours = 2,
            UserCanChangeEmailOnceInHowManyHours = 24
        };
        _optionsMock.Value.Returns(emailOptions);

        _policy = new EmailPolicy(
            _optionsMock,
            _userRepositoryMock,
            _emailVerificationAttemptRepositoryMock,
            _userDeactivationRepositoryMock
        );
    }

    private static readonly User user = new()
    {
        Id = Guid.CreateVersion7(),
        Email = "old@email.com",
        IsEmailVerified = true,
        Roles = [ Role.Customer ],
        IsDeleted = false
    };

    [Fact]
    public async Task EmailPolicy_ShouldDenyChange_WhenUserIsDeleted()
    {
        //Arrange
        user.IsDeleted = true;
        var context = new EmailChangeContext(user, "new@email.com");

        //Act
        var result = await _policy.IsEmailChangeAllowed(context);

        //Assert
        result.Error.Should().Be(DomainErrors.User.NotFound);
    }
    
    [Fact]
    public async Task EmailPolicy_ShouldDenyChange_WhenUserIsDeactivated()
    {
        //Arrange
        user.IsDeleted = false;
        _userDeactivationRepositoryMock.GetLatestAsync(user.Id).Returns(new UserDeactivation());
        var context = new EmailChangeContext(user, "new@email.com");

        //Act
        var result = await _policy.IsEmailChangeAllowed(context);

        //Assert
        result.Error.Should().Be(DomainErrors.User.Deactivated);
    }

    [Fact]
    public async Task EmailPolicy_ShouldDenyChange_WhenNewEmailIsTheSameAsOld()
    {
        //Arrange
        user.IsDeleted = false;
        _userDeactivationRepositoryMock.GetLatestAsync(user.Id).Returns((UserDeactivation)null!);
        var context = new EmailChangeContext(user, user.Email);

        //Act
        var result = await _policy.IsEmailChangeAllowed(context);

        //Assert
        result.Error.Should().Be(DomainErrors.EmailChange.TheSameEmail);
    }

    [Fact]
    public async Task EmailPolicy_ShouldDenyChange_WhenNewEmailIsAlreadyTaken()
    {
        //Arrange
        user.IsDeleted = false;
        _userDeactivationRepositoryMock.GetLatestAsync(user.Id).Returns((UserDeactivation)null!);
        var context = new EmailChangeContext(user, "new@email.com");
        _userRepositoryMock.CountUsersWithEmailAsync("new@email.com").Returns(1);

        //Act
        var result = await _policy.IsEmailChangeAllowed(context);

        //Assert
        result.Error.Should().Be(DomainErrors.Email.EmailAlreadyInUse);
    }

    [Fact]
    public async Task EmailPolicy_ShouldDenyChange_WhenTooManyAttempts()
    {
        //Arrange
        user.IsDeleted = false;
        _userDeactivationRepositoryMock.GetLatestAsync(user.Id).Returns((UserDeactivation)null!);
        var context = new EmailChangeContext(user, "new@email.com");
        _userRepositoryMock.CountUsersWithEmailAsync("new@email.com").Returns(0);
        _emailVerificationAttemptRepositoryMock.GetLastSuccessfulAttemptAsync(user.Email)
            .Returns(new EmailVerificationAttempt() { AttemptedAt = DateTime.UtcNow });

        //Act
        var result = await _policy.IsEmailChangeAllowed(context);

        //Assert
        result.Error.Should().Be(DomainErrors.EmailChange.TooOften);
    }

    [Fact]
    public async Task EmailPolicy_ShouldAllowChange_WhenAllRulesApply()
    {
        //Arrange
        user.IsDeleted = false;
        _userDeactivationRepositoryMock.GetLatestAsync(user.Id).Returns((UserDeactivation)null!);
        var context = new EmailChangeContext(user, "new@email.com");
        _userRepositoryMock.CountUsersWithEmailAsync("new@email.com").Returns(0);
        _emailVerificationAttemptRepositoryMock.GetLastSuccessfulAttemptAsync(user.Email)
            .Returns(new EmailVerificationAttempt() { AttemptedAt = DateTime.UtcNow.AddYears(-100) });

        //Act
        var result = await _policy.IsEmailChangeAllowed(context);

        //Assert
        result.Should().Be(PolicyResult.Success);
    }
}