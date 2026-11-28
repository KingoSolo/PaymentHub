using System.Text;
using Microsoft.EntityFrameworkCore;
using PaymentHub.Shared.Data;
using RabbitMQ.Client;

namespace PaymentHub.Worker.Services;

public class OutboxPublisher(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxPublisher> logger
): BackgroundService
{
     private IConnection? _connection;
    private IChannel? _channel;

        private async Task ConnectAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
                Port = configuration.GetValue<int>("RabbitMQ:Port", 5672),
                UserName = configuration["RabbitMQ:UserName"]!,
                Password = configuration["RabbitMQ:Password"]!
            };
            _connection = await factory.CreateConnectionAsync(stoppingToken);

            var options = new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true);

             
            _channel = await _connection.CreateChannelAsync(options,stoppingToken);
            await _channel.ExchangeDeclareAsync(
                exchange:"payment.exchange",
                type: ExchangeType.Direct,
                durable:true,
                autoDelete:false,
                cancellationToken:stoppingToken
            );
            await _channel.QueueDeclareAsync(
                queue:"payment.queued",
                durable:true,
                exclusive:false,
                autoDelete:false,
                cancellationToken:stoppingToken
            );
            await _channel.QueueBindAsync(
                queue:"payment.queued",
                exchange:"payment.exchange",
                routingKey:"PaymentQueued",
                cancellationToken:stoppingToken
            );
             
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ConnectAsync(stoppingToken);
                    logger.LogInformation("Connected to RabbitMQ.");
                    break;
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "RabbitMQ unavailable. Retrying in 5 seconds...");
                    try
                    {
                        await Task.Delay(5000, stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        return;
                    }
                }
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PublishPendingMessagesAsync(stoppingToken);
                    await Task.Delay(2000, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred while polling outbox messages.");
                    try
                    {
                        await Task.Delay(2000, stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
        }


        private async Task PublishPendingMessagesAsync(CancellationToken stoppingToken)
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var pendingMessages = await dbContext.OutboxMessages
                .Where(m => m.PublishedAt == null)
                .OrderBy(m => m.CreatedAt)
                .Take(20)
                .ToListAsync(stoppingToken);
                
            if (pendingMessages.Count == 0)
            {
                return;
            }
            
            foreach (var message in pendingMessages)
            {
                try
                {
                    message.AttemptCount++;
                    var body = Encoding.UTF8.GetBytes(message.Payload);

                    var properties = new BasicProperties
                    {
                        Persistent = true,
                        MessageId = message.Id.ToString(),
                        Type = message.Type
                    };

                    await _channel!.BasicPublishAsync(
                        exchange: "payment.exchange",
                        routingKey: message.Type,
                        mandatory: true,
                        basicProperties: properties,
                        body: body,
                        cancellationToken: stoppingToken);

                        message.PublishedAt = DateTimeOffset.UtcNow;
                        message.LastError = null;

                        logger.LogInformation(
                            "Published OutboxMessage {MessageId} for Payment {PaymentId}.",
                            message.Id, message.PaymentId);
                    }
                    catch (Exception ex)
                    {
                        message.LastError = ex.Message;

                        logger.LogWarning(
                            ex,
                            "Failed to publish OutboxMessage {MessageId} (Attempt {AttemptCount}).",
                            message.Id, message.AttemptCount);
                    }
                }
                 await dbContext.SaveChangesAsync(stoppingToken);
            }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);

            if (_channel is not null)
                await _channel.DisposeAsync();

            if (_connection is not null)
                await _connection.DisposeAsync();

            await base.StopAsync(cancellationToken);
        }
       
}