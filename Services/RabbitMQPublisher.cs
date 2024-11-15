using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace RabbitMQWeb.Watermark.Services
{
    public class RabbitMQPublisher
    {
        private readonly RabbitMQService _rabbitmqService;

        public RabbitMQPublisher(RabbitMQService rabbitmqService)
        {
            _rabbitmqService = rabbitmqService;
        }

        public void Publish(ProductImageCreatedEvent productImageCreatedEvent)
        {
            var channel = _rabbitmqService.Connect();

            var bodyString = JsonSerializer.Serialize(productImageCreatedEvent);

            var bodyByte = Encoding.UTF8.GetBytes(bodyString);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(RabbitMQService.ExchangeName, RabbitMQService.RoutingWatermak, basicProperties: properties, body: bodyByte);
        }
    }
}
