using FluentAssertions;
using NSubstitute;
using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Models;
using UserManagement.Application.Repositories;
using UserManagement.Application.Services;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UnitTests.Services;

public class EmailService_ConfirmRequestTests
{
    private readonly IEmailVerificationAttemptRepository _repositoryMock;
    private readonly IEmailPolicy _emailPolicyMock;
    private readonly IEmailSender _emailSenderMock;
    private readonly IUrlProvider _urlProviderMock;

    private readonly IEmailService _service;

    public EmailService_ConfirmRequestTests()
    {
        _repositoryMock = Substitute.For<IEmailVerificationAttemptRepository>();
        _emailPolicyMock = Substitute.For<IEmailPolicy>();
        _emailSenderMock = Substitute.For<IEmailSender>();
        _urlProviderMock = Substitute.For<IUrlProvider>();

        _service = new EmailService(
            _repositoryMock,
            _emailPolicyMock,
            _emailSenderMock,
            _urlProviderMock
        );
    }

    private static readonly User user = new()
    {
        Id = Guid.CreateVersion7(),
        Email = "user@email.com",
        IsEmailVerified = false
    };

    [Fact]
    public async Task EmailService_ShouldReturnErrorOnConfirmRequest_WhenAttemptIsNotFound()
    {
        //Arrange
        var verificationCode = "code";
        _repositoryMock.GetAsync(verificationCode).Returns((EmailVerificationAttempt)null!);

        //Act
        var result = await _service.ConfirmRequestAsync(verificationCode);

        //Assert
        result.Error.Should().Be(DomainErrors.EmailVerification.CodeExpiredOrNotFound);
    }

    [Fact]
    public async Task EmailService_ShouldReturnErrorOnConfirmRequest_WhenPolicyDenies()
    {
        //Arrange
        var verificationCode = "code";
        var attemptRecord = new EmailVerificationAttempt()
        {
            User = user,
            Email = user.Email,
            IsSucceeded = false,
            SucceededAt = null,
            PreviousEmail = null
        };
        _repositoryMock.GetAsync(verificationCode).Returns(attemptRecord);
        var error = DomainErrors.EmailVerification.CodeExpiredOrNotFound;
        _emailPolicyMock.IsConfirmationAllowedAsync(attemptRecord).Returns(error);

        //Act
        var result = await _service.ConfirmRequestAsync(verificationCode);

        //Assert
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task EmailService_ShouldReturnSuccessOnConfirmRequest_WhenPolicyAllowsAndAttemptIsAccountConfirmation()
    {
        //Arrange
        var verificationCode = "code";
        var attemptRecord = new EmailVerificationAttempt()
        {
            User = user,
            Email = user.Email,
            IsSucceeded = false,
            SucceededAt = null,
            PreviousEmail = null
        };
        _repositoryMock.GetAsync(verificationCode).Returns(attemptRecord);
        _emailPolicyMock.IsConfirmationAllowedAsync(attemptRecord).Returns(PolicyResult.Success);

        //Act
        var result = await _service.ConfirmRequestAsync(verificationCode);

        //Assert
        result.IsSuccess.Should().BeTrue();
        attemptRecord.IsSucceeded.Should().BeTrue();
        user.IsEmailVerified.Should().BeTrue();
    }

    [Fact]
    public async Task EmailService_ShouldReturnSuccessOnConfirmRequest_WhenPolicyAllowsAndAttemptIsEmailChange()
    {
        //Arrange
        var verificationCode = "code";
        var newEmail = "new@email.com";
        var attemptRecord = new EmailVerificationAttempt()
        {
            User = user,
            Email = newEmail,
            IsSucceeded = false,
            SucceededAt = null,
            PreviousEmail = user.Email
        };
        _repositoryMock.GetAsync(verificationCode).Returns(attemptRecord);
        _emailPolicyMock.IsConfirmationAllowedAsync(attemptRecord).Returns(PolicyResult.Success);

        //Act
        var result = await _service.ConfirmRequestAsync(verificationCode);

        //Assert
        result.IsSuccess.Should().BeTrue();
        attemptRecord.IsSucceeded.Should().BeTrue();
        user.Email.Should().Be(newEmail);
    }
}