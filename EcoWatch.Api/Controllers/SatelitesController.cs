using EcoWatch.Domain.Documents;
using EcoWatch.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace EcoWatch.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SatelitesController : ControllerBase
    {
        private readonly MongoDbContext _mongoContext;

        public SatelitesController(MongoDbContext mongoContext)
        {
            _mongoContext = mongoContext;
        }

        [HttpPost("telemetria")]
        public async Task<IActionResult> ReceberDadosSatelite([FromBody] JsonElement payloadBruto)
        {
            if (payloadBruto.ValueKind == JsonValueKind.Undefined || payloadBruto.ValueKind == JsonValueKind.Null)
                return BadRequest("O payload não pode estar vazio.");

            var jsonString = payloadBruto.GetRawText();
            var documentoBson = BsonDocument.Parse(jsonString);

            var alerta = new AlertaSatelite
            {
                Fonte = "Satelite-INPE",
                DataCapturaUtc = DateTime.UtcNow,
                DadosBrutos = documentoBson
            };

            await _mongoContext.AlertasSatelite.InsertOneAsync(alerta);

            return StatusCode(201, new { message = "Telemetria registada com sucesso no MongoDB.", id = alerta.Id });
        }
    }
}