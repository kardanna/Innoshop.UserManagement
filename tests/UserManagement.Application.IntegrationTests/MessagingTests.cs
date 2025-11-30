using System.Text;
using System.Text.Json;
using FluentAssertions;
using Innoshop.Contracts.UserManagement.UserEvents;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using UserManagement.Application.UseCases.Users.Deactivate;
using UserManagement.Application.UseCases.Users.Delete;
using UserManagement.Application.UseCases.Users.Reactivate;
using UserManagement.Application.UseCases.Users.Register;

namespace UserManagement.Application.IntegrationTests;

[Collection(IntegrationTestCollection.CollectionName)]
public class MessagingTests : BaseIntegrationTest
{
    public MessagingTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    private readonly string exchangeName = Exchange.Name;

    [Fact]
    public async Task Delete_Should_PublishMessageToRabbitMQ()
    {
        //Arrange
        var connection = await _rabbitMQConnectionProvider.GetConnectionAsync(CancellationToken.None);
        var channel = await connection.CreateChannelAsync();
        var queue = await channel.QueueDeclareAsync();
        await channel.QueueBindAsync(
            queue: queue.QueueName,
            exchange: exchangeName,
            routingKey: UserDeletedMessage.Topic
        );
        
        var tcs = new TaskCompletionSource<UserDeletedMessage>();

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var messageString = Encoding.UTF8.GetString(body);
            var message = JsonSerializer.Deserialize<UserDeletedMessage>(messageString);
            tcs.TrySetResult(message!);
        };

        await channel.BasicConsumeAsync(
            queue: queue.QueueName,
            autoAck: true,
            consumer
        );

        var registerUserCommand = new RegisterUserCommand(
            "name",
            "surname",
            DateOnly.FromDateTime(DateTime.Now.AddYears(-50)),
            "deleteMessage@email.com",
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
        var message = await tcs.Task;
        
        //Assert
        message.UserId.Should().Be(createdUserId);

        try
        {
            await channel.CloseAsync();
        }
        catch (AlreadyClosedException) { }
    }

    [Fact]
    public async Task Deactivate_Should_PublishMessageToRabbitMQ()
    {
        //Arrange
        var connection = await _rabbitMQConnectionProvider.GetConnectionAsync(CancellationToken.None);
        var channel = await connection.CreateChannelAsync();
        var queue = await channel.QueueDeclareAsync();
        await channel.QueueBindAsync(
            queue: queue.QueueName,
            exchange: exchangeName,
            routingKey: UserDeactivatedMessage.Topic
        );
        
        var tcs = new TaskCompletionSource<UserDeactivatedMessage>();

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var messageString = Encoding.UTF8.GetString(body);
            var message = JsonSerializer.Deserialize<UserDeactivatedMessage>(messageString);
            tcs.TrySetResult(message!);
        };

        await channel.BasicConsumeAsync(
            queue: queue.QueueName,
            autoAck: true,
            consumer
        );

        var registerUserCommand = new RegisterUserCommand(
            "name",
            "surname",
            DateOnly.FromDateTime(DateTime.Now.AddYears(-50)),
            "deactivateMessage@email.com",
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
        var message = await tcs.Task;

        //Assert
        message.UserId.Should().Be(createdUserId);
        
        try
        {
            await channel.CloseAsync();
        }
        catch (AlreadyClosedException) { }
    }

    [Fact]
    public async Task Reactivate_Should_PublishMessageToRabbitMQ()
    {
        //Arrange
        var connection = await _rabbitMQConnectionProvider.GetConnectionAsync(CancellationToken.None);
        var channel = await connection.CreateChannelAsync();
        var queue = await channel.QueueDeclareAsync();
        await channel.QueueBindAsync(
            queue: queue.QueueName,
            exchange: exchangeName,
            routingKey: UserReactivatedMessage.Topic
        );
        
        var tcs = new TaskCompletionSource<UserReactivatedMessage>();

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var messageString = Encoding.UTF8.GetString(body);
            var message = JsonSerializer.Deserialize<UserReactivatedMessage>(messageString);
            tcs.TrySetResult(message!);
        };

        await channel.BasicConsumeAsync(
            queue: queue.QueueName,
            autoAck: true,
            consumer
        );
        
        var registerUserCommand = new RegisterUserCommand(
            "name",
            "surname",
            DateOnly.FromDateTime(DateTime.Now.AddYears(-50)),
            "reactivateMessage@email.com",
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
        var message = await tcs.Task;

        //Assert
        message.UserId.Should().Be(createdUserId);
        
        try
        {
            await channel.CloseAsync();
        }
        catch (AlreadyClosedException) { }
    }
}