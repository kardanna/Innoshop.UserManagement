using FluentAssertions;
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

public class EmailPolicy_ConfirmationTests
{
    private readonly IOptions<EmailOptions> _optionsMock;
    private readonly IUserRepository _userRepositoryMock;
    private readonly IEmailVerificationAttemptRepository _emailVerificationAttemptRepositoryMock;
    private readonly IUserDeactivationRepository _userDeactivationRepositoryMock;

    private readonly IEmailPolicy _policy;

    public EmailPolicy_ConfirmationTests()
    {
        _optionsMock = Substitute.For<IOptions<EmailOptions>>();
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _emailVerificationAttemptRepositoryMock = Substitute.For<IEmailVerificationAttemptRepository>();
        _userDeactivationRepositoryMock = Substitute.For<IUserDeactivationRepository>();

        var emailOptions = new EmailOptions()
        { 
            VerificationCodeLifetimeInHours = 2,
            UserCanChangeEmailOnceInHowManyHours = 1
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
        IsEmailVerified = false,
        Roles = [ Role.Customer ],
        IsDeleted = false
    };

    private static readonly EmailVerificationAttempt attempt = new()
    {
        VerificationCode = "code",
        Email = "new@email.com",
        PreviousEmail = "old@email.com",
        IsSucceeded = false
    };

    [Fact]
    public async Task EmailPolicy_ShouldDenyConfirmation_WhenAttemptIsAlreadySucceeded()
    {
        //Arrange
        attempt.IsSucceeded = true;

        //Act
        var result = await _policy.IsConfirmationAllowedAsync(attempt);

        //Assert
        result.Error.Should().Be(DomainErrors.EmailVerification.CodeExpiredOrNotFound);
    }

    [Fact]
    public async Task EmailPolicy_ShouldDenyConfirmation_WhenAttemptCodeHasExpired()
    {
        //Arrange
        attempt.IsSucceeded = false;
        attempt.AttemptedAt = DateTime.UtcNow.AddYears(-100);

        //Act
        var result = await _policy.IsConfirmationAllowedAsync(attempt);

        //Assert
        result.Error.Should().Be(DomainErrors.EmailVerification.CodeExpiredOrNotFound);
    }

    [Fact]
    public async Task EmailPolicy_ShouldDenyConfirmation_WhenAttemptIsEmailChangeAttemptAndEmailIsTaken()
    {
        //Arrange
        attempt.IsSucceeded = false;
        attempt.AttemptedAt = DateTime.UtcNow;
        attempt.PreviousEmail = "old@email.com";
        attempt.Email = "new@email.com";
        user.Email = "old@email.com";
        attempt.User = user;
        _userRepositoryMock.CountUsersWithEmailAsync(attempt.Email).Returns(1);

        //Act
        var result = await _policy.IsConfirmationAllowedAsync(attempt);

        //Assert
        result.Error.Should().Be(DomainErrors.Email.EmailAlreadyInUse);
    }

    [Fact]
    public async Task EmailPolicy_ShouldAllowConfirmation_WhenAttemptIsAccountVerificationAttemptAndEmailIsTaken()
    {
        //Arrange
        attempt.IsSucceeded = false;
        attempt.AttemptedAt = DateTime.UtcNow;
        attempt.PreviousEmail = null;
        attempt.Email = "old@email.com";
        user.Email = "old@email.com";
        attempt.User = user;
        _userRepositoryMock.CountUsersWithEmailAsync(attempt.Email).Returns(1);

        //Act
        var result = await _policy.IsConfirmationAllowedAsync(attempt);

        //Assert
        result.Should().Be(PolicyResult.Success);
    }
}