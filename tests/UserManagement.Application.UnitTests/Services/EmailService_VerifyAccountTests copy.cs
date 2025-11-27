using FluentAssertions;
using NSubstitute;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Repositories;
using UserManagement.Application.Services;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UnitTests.Services;

public class EmailService_VerifyAccountTests
{
    private readonly IEmailVerificationAttemptRepository _repositoryMock;
    private readonly IEmailPolicy _emailPolicyMock;
    private readonly IEmailSender _emailSenderMock;
    private readonly IUrlProvider _urlProviderMock;

    private readonly IEmailService _service;

    public EmailService_VerifyAccountTests()
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
        Email = "user@email.com"
    };

    [Fact]
    public async Task EmailService_ShouldReturnSuccessOnVerifyAccount_WhenEmailIsSuccessfullySent()
    {
        //Arrange
        _emailSenderMock.SendAccountVerificationMessageAsync(user.Email, Arg.Any<string>(), Arg.Any<string?>())
            .Returns(Result.Success());

        //Act
        var result = await _service.VerifyAccountAsync(user);

        //Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Received(1).Add(
            Arg.Is<EmailVerificationAttempt>(a => a.User == user && a.Email == user.Email));
        await _emailSenderMock.Received(1).SendAccountVerificationMessageAsync(user.Email, Arg.Any<string>(), Arg.Any<string?>());
    }

    [Fact]
    public async Task EmailService_ShouldReturnErrorOnVerifyAccount_WhenEmailWasNotSent()
    {
        //Arrange
        var error = DomainErrors.Email.FailedToSend;
        _emailSenderMock.SendAccountVerificationMessageAsync(user.Email, Arg.Any<string>(), Arg.Any<string?>())
            .Returns(Result.Failure(error));

        //Act
        var result = await _service.VerifyAccountAsync(user);

        //Assert
        result.Error.Should().Be(error);
        _repositoryMock.Received(1).Add(
            Arg.Is<EmailVerificationAttempt>(a => a.User == user && a.Email == user.Email));
        await _emailSenderMock.Received(1).SendAccountVerificationMessageAsync(user.Email, Arg.Any<string>(), Arg.Any<string?>());
    }
}