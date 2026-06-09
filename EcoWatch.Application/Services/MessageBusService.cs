using EcoWatch.Application.Services;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace EcoWatch.Infrastructure.Services
{
    public class MessageBusService : IMessageBusService, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private const string ExchangeName = "ecowatch_alertas_topic";

        public MessageBusService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("CloudAMQP")
                ?? configuration["RabbitMq:ConnectionString"];

            var factory = new ConnectionFactory
            {
                Uri = new Uri(connectionString)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Topic, durable: true);
        }

        public async Task PublicarAlertaIncendioAsync(object ocorrenciaPayload)
        {
            var routingKey = "alerta.novo";
            PublicarMensagem(routingKey, ocorrenciaPayload);
            await Task.CompletedTask;
        }

        public async Task PublicarImagemRecebidaAsync(object eventoTelemetria)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(eventoTelemetria);
            var body = System.Text.Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(
                exchange: "ecowatch_alertas_topic",
                routingKey: "telemetria.nova",
                basicProperties: null,
                body: body
            );

            await Task.CompletedTask;
        }

        private void PublicarMensagem(string routingKey, object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body
            );
        }

        public void Dispose()
        {
            if (_channel?.IsOpen == true) _channel.Close();
            if (_connection?.IsOpen == true) _connection.Close();
        }
    }
}