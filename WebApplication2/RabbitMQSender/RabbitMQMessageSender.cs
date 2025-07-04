using Microsoft.EntityFrameworkCore.Storage.Json;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Channels;

namespace RabbitAdoption.ProducerAPI.RabbitMQSender
{
    public class RabbitMQMessageSender : IRabbitMQMessageSender
    {
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _deadLetterExchange;
        private IConnection _connection;
        public RabbitMQMessageSender(IConfiguration configuration)
        {
            _hostName = configuration.GetValue<string>("RabbitMqConnection:HostName") ?? "localhost";
            _userName = configuration.GetValue<string>("RabbitMqConnection:UserName") ?? "guest";
            _password = configuration.GetValue<string>("RabbitMqConnection:Password") ?? "guest";
            _deadLetterExchange = configuration.GetValue<string>("TopicAndQueueNames:deadLetterExchange") ?? "adoptionsRabbit.dlx";
        }
        public void SendMessage(object message, string queueName, byte priority = 0)
        {
            if (ConnectionExists())
            {
                using var channel = _connection.CreateModel();
                //channel.QueueDeclare(queueName, false, false, false, null);
                channel.QueueDeclare(queue: queueName,
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: new Dictionary<string, object>
                                  {
                                      {"x-max-priority", 10},
                                      {"x-dead-letter-exchange", _deadLetterExchange}
                                  });

                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.Priority = priority;

                channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: properties, body: body);
            }
        }

        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostName,
                    Password = _password,
                    UserName = _userName,
                };
                _connection = factory.CreateConnection();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RabbitMQ Connection Error] {ex.Message}");
            }
        }

        private bool ConnectionExists()
        {
            if (_connection == null)
            {
                CreateConnection();
                //return true;
            }
            return true;
        }
    }
}
