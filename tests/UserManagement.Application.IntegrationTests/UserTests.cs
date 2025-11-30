using FluentAssertions;
using Innoshop.Contracts.UserManagement.UserRoles;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Services;
using UserManagement.Application.UseCases.Admins.Register;
using UserManagement.Application.UseCases.Users.Deactivate;
using UserManagement.Application.UseCases.Users.Delete;
using UserManagement.Application.UseCases.Users.Get;
using UserManagement.Application.UseCases.Users.Login;
using UserManagement.Application.UseCases.Users.Logout;
using UserManagement.Application.UseCases.Users.LogoutEverywhere;
using UserManagement.Application.UseCases.Users.Reactivate;
using UserManagement.Application.UseCases.Users.Register;
using UserManagement.Application.UseCases.Users.Update;

namespace UserManagement.Application.IntegrationTests;

[Collection(IntegrationTestCollection.CollectionName)]
public class UserTests : BaseIntegrationTest
{
    public UserTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    private const string RegisteredUserEmail = "ivan.ivanov@gmail.com";
    private const string RegisteredUserPassword = "IvanIvanov123";
    private const string RegisteredUserGuidString = "160be924-907f-4d70-d15c-08de2383d454";
    private const string RegisteredAdminEmail = "admin@innoshop.by";
    private const string RegisteredAdminPassword = "Admin123";
    private const string RegisteredAdminGuidString = "30fc2d9e-3bb0-4bdc-d15b-08de2383d454";


    [Fact]
    public async Task RegisterUser_ShouldAdd_NewUserToDatabase()
    {
        //Arrange
        var registerUserCommand = new RegisterUserCommand(
            "name",
            "surname",
            DateOnly.FromDateTime(DateTime.Now.AddYears(-50)),
            "register@email.com",
            "Password1!"
        );

        //Act
        var response = await _sender.Send(registerUserCommand);
        var createdUserId = response.Value.Id;
        var user = await _appContext.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == createdUserId);

        //Assert
        user.Should().NotBeNull();
        user.FirstName.Should().Be(registerUserCommand.FirstName);
        user.LastName.Should().Be(registerUserCommand.LastName);
        user.DateOfBirth.Should().Be(registerUserCommand.DateOfBirth);
        user.Email.Should().Be(registerUserCommand.Email);
        user.Roles.Should().Contain(r => r.Name == Role.Customer.Name);
    }

    [Fact]
    public async Task RegisterAdmin_ShouldAdd_NewAdminToDatabase()
    {
        //Arrange
        var registerAdminCommand = new RegisterAdminCommand(
            "name",
            "surname",
            DateOnly.FromDateTime(DateTime.Now.AddYears(-50)),
            "registeradmin@email.com",
            "Password1!",
            Guid.Parse(RegisteredAdminGuidString)
        );

        //Act
        var response = await _sender.Send(registerAdminCommand);
        var createdAdminId = response.Value.Id;
        var user = await _appContext.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == createdAdminId);

        //Assert
        user.Should().NotBeNull();
        user.FirstName.Should().Be(registerAdminCommand.FirstName);
        user.LastName.Should().Be(registerAdminCommand.LastName);
        user.DateOfBirth.Should().Be(registerAdminCommand.DateOfBirth);
        user.Email.Should().Be(registerAdminCommand.Email);
        user.Roles.Should().Contain(r => r.Name == Role.Administrator.Name);
    }

    [Fact]
    public async Task Login_ShouldAdd_NewTokenRecordAndLoginAttemptToDatabase()
    {
        //Arrange
        var loginUserCommand = new LoginUserCommand(
            RegisteredUserEmail,
            RegisteredUserPassword,
            "deviceFingerprint"
        );

        //Act
        var response = await _sender.Send(loginUserCommand);
        var generatedRefreshToken = response.Value.RefreshToken;
        var tokenRecord = await _appContext.TokenRecords.FirstOrDefaultAsync(r => r.RefreshToken == generatedRefreshToken);
        var loginAttempt = await _appContext.LoginAttempts.FirstOrDefaultAsync(a => a.Email == loginUserCommand.Email);

        //Assert
        tokenRecord.Should().NotBeNull();
        tokenRecord.UserId.Should().Be(Guid.Parse(RegisteredUserGuidString));
        loginAttempt.Should().NotBeNull();
        loginAttempt.DeviceFingerprint.Should().Be(loginUserCommand.DeviceFingerprint);
    }

    [Fact]
    public async Task Logout_Should_RevokeTokenAndDeleteTokenRecordFromDatabase()
    {
        //Arrange
        var loginUserCommand = new LoginUserCommand(
            RegisteredUserEmail,
            RegisteredUserPassword,
            "deviceFingerprint"
        );
        var loginUserCommandResponse = await _sender.Send(loginUserCommand);
        var createdTokenRecord = await _appContext.TokenRecords
            .FirstOrDefaultAsync(r => r.RefreshToken == loginUserCommandResponse.Value.RefreshToken);
        var accessTokenId = createdTokenRecord!.AccessTokenId;
        var logoutUserCommand = new LogoutUserCommand(accessTokenId);
        
        //Act
        await _sender.Send(logoutUserCommand);
        var deletedTokenRecord = await _appContext.TokenRecords
            .FirstOrDefaultAsync(r => r.AccessTokenId == accessTokenId);

        //Assert
        deletedTokenRecord.Should().BeNull();
    }

    [Fact]
    public async Task LogoutEverywhere_Should_RevokeAllTokensAndDeleteTokenRecordsFromDatabase()
    {
        //Arrange
        var loginUserCommand = new LoginUserCommand(
            RegisteredUserEmail,
            RegisteredUserPassword,
            "deviceFingerprint"
        );
        var userId = Guid.Parse(RegisteredUserGuidString);
        await _sender.Send(loginUserCommand);
        var logoutEverywhereUserCommand = new LogoutUserEverywhereCommand(userId);
        
        //Act
        await _sender.Send(logoutEverywhereUserCommand);
        var userTokenRecords = await _appContext.TokenRecords
            .Where(r => r.UserId == userId)
            .ToListAsync();

        //Assert
        userTokenRecords.Should().BeEmpty();
    }

    [Fact]
    public async Task Update_Should_UpdateUserRecordInTheDatabase()
    {
        //Arrange
        var userId = Guid.Parse(RegisteredUserGuidString);
        var updateUserCommand = new UpdateUserCommand(
            userId,
            "new name",
            "new surname",
            DateOnly.FromDateTime(DateTime.Now.AddYears(-50))
        );
        
        //Act
        await _sender.Send(updateUserCommand);
        var user = await _appContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        //Assert
        user.Should().NotBeNull();
        user.FirstName.Should().Be(updateUserCommand.FirstName);
        user.LastName.Should().Be(updateUserCommand.LastName);
        user.DateOfBirth.Should().Be(updateUserCommand.DateOfBirth);
    }

    [Fact]
    public async Task Get_Should_ReturnUserFromTheDatabase()
    {
        //Arrange
        var userId = Guid.Parse(RegisteredUserGuidString);
        var getUserQuery = new GetUserQuery(userId);
        
        //Act
        var response = await _sender.Send(getUserQuery);
        var user = await _appContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        //Assert
        user.Should().NotBeNull();
        response.Value.Email.Should().Be(user.Email);
        response.Value.FirstName.Should().Be(user.FirstName);
        response.Value.LastName.Should().Be(user.LastName);
    }

    [Fact]
    public async Task Delete_Should_MarkUserAsDeletedInTheDatabase()
    {
        //Arrange
        var registerUserCommand = new RegisterUserCommand(
            "name",
            "surname",
            DateOnly.FromDateTime(DateTime.Now.AddYears(-50)),
            "delete@email.com",
            "Password1!"
        );
        var response = await _sender.Send(registerUserCommand);
        var createdUserId = response.Value.Id;
        var deleteUserCommand = new DeleteUserCommand(
            createdUserId,
            registerUserCommand.Password,
            createdUserId
        );

        //Act
        await _sender.Send(deleteUserCommand);
        var user = await _appContext.Users.FirstOrDefaultAsync(u => u.Id == createdUserId);

        //Assert
        user.Should().NotBeNull();
        user.IsDeleted.Should().BeTrue();
        user.FirstName.Should().NotBe(registerUserCommand.FirstName);
        user.LastName.Should().NotBe(registerUserCommand.LastName);
        user.DateOfBirth.Should().NotBe(registerUserCommand.DateOfBirth);
        user.Email.Should().NotBe(registerUserCommand.Email);
    }

    [Fact]
    public async Task Deactivate_Should_MarkUserAsDeactivatedInTheDatabase()
    {
        //Arrange
        var registerUserCommand = new RegisterUserCommand(
            "name",
            "surname",
            DateOnly.FromDateTime(DateTime.Now.AddYears(-50)),
            "deactivate@email.com",
            "Password1!"
        );
        var response = await _sender.Send(registerUserCommand);
        var createdUserId = response.Value.Id;
        var deactivateUserCommand = new DeactivateUserCommand(
            createdUserId,
            createdUserId
        );

        //Act
        await _sender.Send(deactivateUserCommand);
        var deactivateRecord = await _appContext.UserDeactivations
            .Include(r => r.User)
                .ThenInclude(u => u.Roles)
            .Include(r => r.DeactivationRequester)
                .ThenInclude(u => u.Roles)
            .Include(r => r.ReactivationRequester)
                .ThenInclude(u => u!.Roles)
            .Where(r => r.UserId == createdUserId)
            .OrderByDescending(r => r.DeactivatedAt)
            .FirstOrDefaultAsync();

        //Assert
        deactivateRecord.Should().NotBeNull();
        deactivateRecord.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Reactivate_Should_MarkUserAsReactivatedInTheDatabase()
    {
        //Arrange
        var registerUserCommand = new RegisterUserCommand(
            "name",
            "surname",
            DateOnly.FromDateTime(DateTime.Now.AddYears(-50)),
            "reactivate@email.com",
            "Password1!"
        );
        var response = await _sender.Send(registerUserCommand);
        var createdUserId = response.Value.Id;
        var deactivateUserCommand = new DeactivateUserCommand(
            createdUserId,
            createdUserId
        );
        await _sender.Send(deactivateUserCommand);
        var reactivateUserCommand = new ReactivateUserCommand(
            createdUserId,
            createdUserId
        );

        //Act
        await _sender.Send(reactivateUserCommand);
        var deactivateRecord = await _appContext.UserDeactivations
            .Include(r => r.User)
                .ThenInclude(u => u.Roles)
            .Include(r => r.DeactivationRequester)
                .ThenInclude(u => u.Roles)
            .Include(r => r.ReactivationRequester)
                .ThenInclude(u => u!.Roles)
            .Where(r => r.UserId == createdUserId)
            .OrderByDescending(r => r.DeactivatedAt)
            .FirstOrDefaultAsync();

        //Assert
        deactivateRecord.Should().NotBeNull();
        deactivateRecord.IsActive.Should().BeTrue();
    }
}