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

        public async Task PublicarAlertaIncendioAsync(object ocorrenciaPayload)
        {
            var factory = new ConnectionFactory { Uri = new Uri(_connectionString) };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: "alertas_incendio_queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var mensagem = JsonSerializer.Serialize(ocorrenciaPayload);
            var corpoMensagem = Encoding.UTF8.GetBytes(mensagem);

            var properties = new BasicProperties
            {
                Persistent = true
            };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: "alertas_incendio_queue",
                mandatory: false,
                basicProperties: properties,
                body: corpoMensagem);
        }
    }
}