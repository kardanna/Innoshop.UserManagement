using FluentAssertions;
using NSubstitute;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Repositories;
using UserManagement.Application.Services;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UnitTests.Services;

public class EmailService_ClearUserRecordsTests
{
    private readonly IEmailVerificationAttemptRepository _repositoryMock;
    private readonly IEmailPolicy _emailPolicyMock;
    private readonly IEmailSender _emailSenderMock;
    private readonly IUrlProvider _urlProviderMock;

    private readonly IEmailService _service;

    public EmailService_ClearUserRecordsTests()
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
    public async Task EmailService_ShouldCallRepositoryOnClearUserRecords_Always()
    {
        //Arrange
        var userId = Guid.CreateVersion7();

        //Act
        await _service.ClearUserRecordsAsync(userId);

        //Assert
        _repositoryMock.Received(1).RemoveAllUserAttempts(userId);
    }
}