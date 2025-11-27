using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NSubstitute;
using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Models;
using UserManagement.Application.Policies;
using UserManagement.Application.UseCases.Passwords.Change;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;

namespace UserManagement.Application.UnitTests.Policies;

public class PasswordChangePolicyTests
{
    private readonly IOptions<Options.PasswordOptions> _passwordOptionsMock;
    private readonly IPasswordHasher<User> _hasherMock;

    private readonly IPasswordPolicy _policy;

    public PasswordChangePolicyTests()
    {
        _passwordOptionsMock = Substitute.For<IOptions<Options.PasswordOptions>>();
        _hasherMock = Substitute.For<IPasswordHasher<User>>();

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

    private static readonly ChangePasswordCommand changePasswordCommand = new(
        Guid.CreateVersion7(),
        "oldPassword",
        "newPassword"
    );

    [Fact]
    public async Task PasswordPolicy_ShouldDenyChange_WhenUserIsDeleted()
    {
        //Arrange
        user.IsDeleted = true;
        var context = new ChangePasswordContext(changePasswordCommand);

        //Act
        var result = await _policy.IsPasswordChangeAllowedAsync(user, context);

        //Assert
        result.Error.Should().Be(DomainErrors.User.NotFound);
    }

    [Fact]
    public async Task PasswordPolicy_ShouldDenyChange_WhenOldPasswordDoesNotMatch()
    {
        //Arrange
        user.IsDeleted = false;
        var context = new ChangePasswordContext(changePasswordCommand);
        _hasherMock.VerifyHashedPassword(null!, user.PasswordHash, context.OldPassword).Returns(PasswordVerificationResult.Failed);

        //Act
        var result = await _policy.IsPasswordChangeAllowedAsync(user, context);

        //Assert
        result.Error.Should().Be(DomainErrors.PasswordChange.EmptyOrWrongPassword);
    }

    [Fact]
    public async Task PasswordPolicy_ShouldAllowChange_WhenAllRulesApply()
    {
        //Arrange
        user.IsDeleted = false;
        var context = new ChangePasswordContext(changePasswordCommand);
        _hasherMock.VerifyHashedPassword(null!, user.PasswordHash, context.OldPassword).Returns(PasswordVerificationResult.Success);

        //Act
        var result = await _policy.IsPasswordChangeAllowedAsync(user, context);

        //Assert
        result.Should().Be(PolicyResult.Success);
    }

}