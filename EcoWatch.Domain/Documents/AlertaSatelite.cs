using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace EcoWatch.Domain.Documents
{
    public class AlertaSatelite
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("fonte")]
        public string Fonte { get; set; }

        [BsonElement("dataCapturaUtc")]
        public DateTime DataCapturaUtc { get; set; }

        [BsonElement("dadosBrutos")]
        public BsonDocument DadosBrutos { get; set; }
    }
}