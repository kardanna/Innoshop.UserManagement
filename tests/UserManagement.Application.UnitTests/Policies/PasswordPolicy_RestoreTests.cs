using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NSubstitute;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Models;
using UserManagement.Application.Policies;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;

namespace UserManagement.Application.UnitTests.Policies;

public class PasswordPolicy_RestoreTests
{
    private readonly IOptions<Options.PasswordOptions> _passwordOptionsMock;
    private readonly IPasswordHasher<User> _hasherMock;

    private readonly IPasswordPolicy _policy;

    public PasswordPolicy_RestoreTests()
    {
        _passwordOptionsMock = Substitute.For<IOptions<Options.PasswordOptions>>();
        _hasherMock = Substitute.For<IPasswordHasher<User>>();

        var passwordOptions = new Options.PasswordOptions()
        { 
            PasswordRestoreAttemptLifetimeInHours = 2
        };
        _passwordOptionsMock.Value.Returns(passwordOptions);

        _policy = new PasswordPolicy(
            _passwordOptionsMock,
            _hasherMock
        );
    }

    private static readonly User user = new()
    {
        Id = Guid.CreateVersion7(),
        PasswordHash = "hash",
        Roles = [ Role.Customer ],
        IsDeleted = false
    };

    private static readonly PasswordRestoreAttempt restoreAttempt = new();

    [Fact]
    public async Task PasswordPolicy_ShouldDenyRestore_WhenUserIsDeleted()
    {
        //Arrange
        user.IsDeleted = true;
        restoreAttempt.User = user; 

        //Act
        var result = await _policy.IsPasswordRestoreAllowed(restoreAttempt);

        //Assert
        result.Error.Should().Be(DomainErrors.User.NotFound);
    }

    [Fact]
    public async Task PasswordPolicy_ShouldDenyRestore_WhenAttemptExpired()
    {
        //Arrange
        user.IsDeleted = false;
        restoreAttempt.User = user; 
        restoreAttempt.AttemptedAt = DateTime.UtcNow.AddYears(-100); 

        //Act
        var result = await _policy.IsPasswordRestoreAllowed(restoreAttempt);

        //Assert
        result.Error.Should().Be(DomainErrors.PasswordRestore.InvalidOrExpiredRestoreCode);
    }

    [Fact]
    public async Task PasswordPolicy_ShouldAllowRestore_WhenAllRulesApply()
    {
        //Arrange
        user.IsDeleted = false;
        restoreAttempt.User = user; 
        restoreAttempt.AttemptedAt = DateTime.UtcNow; 

        //Act
        var result = await _policy.IsPasswordRestoreAllowed(restoreAttempt);

        //Assert
        result.Should().Be(PolicyResult.Success);
    }
}