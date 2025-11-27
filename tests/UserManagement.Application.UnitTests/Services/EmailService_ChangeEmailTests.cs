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

public class EmailService_ChangeEmailTests
{
    private readonly IEmailVerificationAttemptRepository _repositoryMock;
    private readonly IEmailPolicy _emailPolicyMock;
    private readonly IEmailSender _emailSenderMock;
    private readonly IUrlProvider _urlProviderMock;

    private readonly IEmailService _service;

    public EmailService_ChangeEmailTests()
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
    public async Task EmailService_ShouldReturnErrorOnChangeEmail_WhenPolicyDenies()
    {
        //Arrange
        var context = new EmailChangeContext(user, "new@email.com");
        var error = DomainErrors.EmailChange.TheSameEmail;
        _emailPolicyMock.IsEmailChangeAllowed(context).Returns(error);

        //Act
        var result = await _service.ChangeEmailAsync(context);

        //Assert
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task EmailService_ShouldReturnErrorOnChangeEmail_WhenEmailWasNotSent()
    {
        //Arrange
        var context = new EmailChangeContext(user, "new@email.com");
        _emailPolicyMock.IsEmailChangeAllowed(context).Returns(PolicyResult.Success);
        var error = DomainErrors.Email.FailedToSend;
        _emailSenderMock.SendEmailAddressVerificationMessageAsync(context.NewEmail, Arg.Any<string>(), Arg.Any<string?>())
            .Returns(Result.Failure(error));

        //Act
        var result = await _service.ChangeEmailAsync(context);

        //Assert
        _repositoryMock.Received(1).RemoveUnseccessfulAttemptsFor(context.User.Email);
        _repositoryMock.Received(1).Add(Arg.Is<EmailVerificationAttempt>(
            a => a.User == user && a.Email == context.NewEmail && a.PreviousEmail == context.User.Email));
        await _emailSenderMock.Received(1).SendEmailAddressVerificationMessageAsync(context.NewEmail, Arg.Any<string>(), Arg.Any<string?>());
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task EmailService_ShouldReturnSuccessOnChangeEmail_WhenPolicyAllowsAndEmailIsSuccessfullySent()
    {
        //Arrange
        var context = new EmailChangeContext(user, "new@email.com");
        _emailPolicyMock.IsEmailChangeAllowed(context).Returns(PolicyResult.Success);
        _emailSenderMock.SendEmailAddressVerificationMessageAsync(context.NewEmail, Arg.Any<string>(), Arg.Any<string?>())
            .Returns(Result.Success());

        //Act
        var result = await _service.ChangeEmailAsync(context);

        //Assert
        _repositoryMock.Received(1).RemoveUnseccessfulAttemptsFor(context.User.Email);
        _repositoryMock.Received(1).Add(Arg.Is<EmailVerificationAttempt>(
            a => a.User == user && a.Email == context.NewEmail && a.PreviousEmail == context.User.Email));
        await _emailSenderMock.Received(1).SendEmailAddressVerificationMessageAsync(context.NewEmail, Arg.Any<string>(), Arg.Any<string?>());
        result.IsSuccess.Should().BeTrue();
    }
}