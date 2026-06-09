using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EcoWatch.Application.Services;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace EcoWatch.Infrastructure.Messaging
{
    public class RabbitMqService : IMessageBusService
    {
        private readonly string _connectionString;

        public RabbitMqService(IConfiguration configuration)
        {
            _connectionString = configuration["RabbitMq:ConnectionString"];

            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new ArgumentNullException("A Connection String do RabbitMQ não foi encontrada na configuração.");
            }
        }

        public Task PublicarAlertaIncendioAsync(object ocorrenciaPayload)
        {
            var factory = new ConnectionFactory { Uri = new Uri(_connectionString) };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: "alertas_incendio_queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var mensagem = JsonSerializer.Serialize(ocorrenciaPayload);
            var corpoMensagem = Encoding.UTF8.GetBytes(mensagem);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(
                exchange: string.Empty,
                routingKey: "alertas_incendio_queue",
                mandatory: false,
                basicProperties: properties,
                body: corpoMensagem);

            return Task.CompletedTask;
        }

        public Task PublicarImagemRecebidaAsync(object eventoTelemetria)
        {
            var factory = new ConnectionFactory { Uri = new Uri(_connectionString) };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: "telemetria_satelite_queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var mensagem = JsonSerializer.Serialize(eventoTelemetria);
            var corpoMensagem = Encoding.UTF8.GetBytes(mensagem);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(
                exchange: string.Empty,
                routingKey: "telemetria_satelite_queue",
                mandatory: false,
                basicProperties: properties,
                body: corpoMensagem);

            return Task.CompletedTask;
        }
    }
}