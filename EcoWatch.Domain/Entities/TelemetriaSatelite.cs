using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EcoWatch.Domain.Entities
{
    public class TelemetriaSatelite
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string ImagemBase64 { get; set; } = string.Empty;

        public DateTime DataIngestaoUtc { get; set; }

        public string StatusProcessamento { get; set; } = "Pendente";
    }
}