using EcoWatch.Api.DTOs.Responses;
using EcoWatch.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EcoWatch.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificacoesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificacoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ObterNotificacoes([FromQuery] double lat, [FromQuery] double lon)
        {
            if (lat == 0 || lon == 0)
                return BadRequest(new { message = "Latitude e longitude atuais são obrigatórias para calcular os alertas." });

            var emailLogado = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == emailLogado);

            if (usuario == null) return Unauthorized();

            var limiteTempo = DateTime.UtcNow.AddHours(-24);
            var ocorrenciasRecentes = await _context.Ocorrencias
                .Where(o => o.DataOcorrenciaUtc >= limiteTempo)
                .ToListAsync();

            var notificacoes = new List<NotificacaoResponseDto>();

            foreach (var ocorrencia in ocorrenciasRecentes)
            {
                var distancia = CalcularDistanciaKm(lat, lon, ocorrencia.Latitude, ocorrencia.Longitude);

                if (distancia <= usuario.RaioAlertasKm)
                {
                    notificacoes.Add(new NotificacaoResponseDto
                    {
                        IdOcorrencia = ocorrencia.Id.ToString(),
                        Titulo = "Alerta de Incêndio",
                        Mensagem = $"Foco de {ocorrencia.TipoOcorrencia.ToLower()} detectado a {Math.Round(distancia, 1)} km de você.",
                        DistanciaKm = Math.Round(distancia, 1),
                        TempoAtras = CalcularTempoAtras(ocorrencia.DataOcorrenciaUtc),
                        NivelUrgencia = "Crítico",
                        Lida = false
                    });
                }
            }

            return Ok(notificacoes.OrderByDescending(n => n.IdOcorrencia));
        }

        private double CalcularDistanciaKm(double lat1, double lon1, double lat2, double lon2)
        {
            var r = 6371;
            var dLat = GrausParaRadianos(lat2 - lat1);
            var dLon = GrausParaRadianos(lon2 - lon1);
            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(GrausParaRadianos(lat1)) * Math.Cos(GrausParaRadianos(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return r * c;
        }

        private double GrausParaRadianos(double graus) => graus * (Math.PI / 180);

        private string CalcularTempoAtras(DateTime dataRegistro)
        {
            var tempo = DateTime.UtcNow - dataRegistro;
            if (tempo.TotalMinutes < 60) return $"{(int)tempo.TotalMinutes}m atrás";
            if (tempo.TotalHours < 24) return $"{(int)tempo.TotalHours}h atrás";
            return $"{(int)tempo.TotalDays}d atrás";
        }
    }
}