using EcoWatch.Domain.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace EcoWatch.Infrastructure.Data
{
    public class MongoDbService
    {
        private readonly IMongoCollection<TelemetriaSatelite> _telemetriaCollection;

        public MongoDbService(IConfiguration configuration)
        {
            var connectionString = configuration.GetSection("MongoDb:ConnectionString").Value;
            var databaseName = configuration.GetSection("MongoDb:DatabaseName").Value;

            var mongoClient = new MongoClient(connectionString);
            var mongoDatabase = mongoClient.GetDatabase(databaseName);

            _telemetriaCollection = mongoDatabase.GetCollection<TelemetriaSatelite>("Telemetria");
        }

        public async Task InserirTelemetriaAsync(TelemetriaSatelite telemetria)
        {
            await _telemetriaCollection.InsertOneAsync(telemetria);
        }
    }
}