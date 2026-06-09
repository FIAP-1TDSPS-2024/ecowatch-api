using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EcoWatch.Infrastructure.Data
{
    public class MongoHealthCheck : IHealthCheck
    {
        private readonly IMongoClient _mongoClient;

        public MongoHealthCheck(IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var ping = new BsonDocument("ping", 1);
                var command = new BsonDocumentCommand<BsonDocument>(ping);

                await _mongoClient.GetDatabase("admin").RunCommandAsync(command, cancellationToken: cancellationToken);

                return HealthCheckResult.Healthy("Conexão com MongoDB está estável.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Falha ao conectar no MongoDB.", ex);
            }
        }
    }
}