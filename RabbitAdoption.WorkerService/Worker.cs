using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitAdoption.WorkerService.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json.Serialization;

namespace RabbitAdoption.WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private IConnection? _connection;
        private IModel? _channel;
        private readonly IConfiguration _configuration;

        public Worker(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            try
            {
                // Validate configuration dependencies
                if (_configuration == null)
                    throw new InvalidOperationException("Configuration service is not available.");

                string hostName = _configuration.GetValue<string>("RabbitMqConnection:HostName") ?? "localhost";
                string userName = _configuration.GetValue<string>("RabbitMqConnection:UserName") ?? "guest";
                string password = _configuration.GetValue<string>("RabbitMqConnection:Password") ?? "guest";
                string? queueName = _configuration.GetValue<string>("TopicAndQueueNames:AdoptionQueue");
                string? deadLetterQueue = _configuration.GetValue<string>("TopicAndQueueNames:deadLetterQueue");
                string? deadLetterExchange = _configuration.GetValue<string>("TopicAndQueueNames:deadLetterExchange");

                if (string.IsNullOrWhiteSpace(queueName))
                    throw new ArgumentException("Queue name is not configured. Please set 'TopicAndQueueNames:AdoptionQueue'.");

                var factory = new ConnectionFactory()
                {
                    HostName = hostName,
                    UserName = userName,
                    Password = password
                };

                // Establish connection
                _connection = factory.CreateConnection() ?? throw new InvalidOperationException("Failed to create RabbitMQ connection.");
                _channel = _connection.CreateModel() ?? throw new InvalidOperationException("Failed to create RabbitMQ channel.");

                // 1. Declare Dead Letter Exchange and Queue
                _channel.ExchangeDeclare(exchange: deadLetterExchange, type: ExchangeType.Fanout);
                _channel.QueueDeclare(queue: deadLetterQueue, durable: true, exclusive: false, autoDelete: false);
                _channel.QueueBind(queue: deadLetterQueue, exchange: deadLetterExchange, routingKey: "");

                // Declare queue
                //_channel.QueueDeclare(queueName,
                //    false, false, false, null);

                // 2. Declare Main Queue with Priority and DLX
                _channel.QueueDeclare(queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: new Dictionary<string, object>
                    {
                        { "x-max-priority", 10 },
                        { "x-dead-letter-exchange", deadLetterExchange }
                    });

                _channel.BasicQos(0, 5, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RabbitMQ Init Error] {ex.Message}");
                throw;
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                try
                {
                    var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                    int adoptionRequest = JsonConvert.DeserializeObject<int>(content);
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<AdoptionRequestProcessor>();
                        processor.ProcessRequestAsync(adoptionRequest).Wait();
                    }

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"[Processing Error] {ex.Message}");
                    _channel.BasicReject(ea.DeliveryTag, requeue: false); // Send to dead letter queue
                }
            };

            _channel.BasicConsume(_configuration.GetValue<string>("TopicAndQueueNames:AdoptionQueue"), false, consumer);
            return Task.CompletedTask;
        }


    }
}
