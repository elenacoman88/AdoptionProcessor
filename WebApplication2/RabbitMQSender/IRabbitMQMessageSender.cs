namespace RabbitAdoption.ProducerAPI.RabbitMQSender
{
    public interface IRabbitMQMessageSender
    {
        void SendMessage(Object message, string queueName, byte priority);
    }
}
