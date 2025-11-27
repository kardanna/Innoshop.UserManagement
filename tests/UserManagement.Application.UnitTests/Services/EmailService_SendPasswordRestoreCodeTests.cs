using FluentAssertions;
using NSubstitute;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Repositories;
using UserManagement.Application.Services;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UnitTests.Services;

public class EmailService_SendPasswordRestoreCodeTests
{
    private readonly IEmailVerificationAttemptRepository _repositoryMock;
    private readonly IEmailPolicy _emailPolicyMock;
    private readonly IEmailSender _emailSenderMock;
    private readonly IUrlProvider _urlProviderMock;

    private readonly IEmailService _service;

    public EmailService_SendPasswordRestoreCodeTests()
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

    [Fact]
    public async Task EmailService_ShouldReturnSuccessOnSendPasswordRestoreCode_WhenEmailIsSuccessfullySent()
    {
        //Arrange
        var email = "user@email.com";
        var code = "code";
        _emailSenderMock.SendPasswordRestorationMessageAsync(email, code, Arg.Any<string?>())
            .Returns(Result.Success());

        //Act
        var result = await _service.SendPasswordRestoreCodeAsync(email, code);

        //Assert
        result.IsSuccess.Should().BeTrue();
        await _emailSenderMock.Received(1).SendPasswordRestorationMessageAsync(email, code, Arg.Any<string?>());
    }

    [Fact]
    public async Task EmailService_ShouldReturnErrorOnSendPasswordRestoreCode_WhenEmailWasNotSent()
    {
        //Arrange
        var email = "user@email.com";
        var code = "code";
        var error = DomainErrors.Email.FailedToSend;
        _emailSenderMock.SendPasswordRestorationMessageAsync(email, code, Arg.Any<string?>())
            .Returns(Result.Failure(error));

        //Act
        var result = await _service.SendPasswordRestoreCodeAsync(email, code);

        //Assert
        result.Error.Should().Be(error);
        await _emailSenderMock.Received(1).SendPasswordRestorationMessageAsync(email, code, Arg.Any<string?>());
    }
}