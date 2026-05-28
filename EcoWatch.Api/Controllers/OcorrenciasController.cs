using EcoWatch.Api.DTOs.Requests;
using EcoWatch.Api.DTOs.Responses;
using EcoWatch.Domain.Entities;
using EcoWatch.Infrastructure.Data;
using EcoWatch.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EcoWatch.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OcorrenciasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMessageBusService _messageBus;

        public OcorrenciasController(ApplicationDbContext context, IMessageBusService messageBus)
        {
            _context = context;
            _messageBus = messageBus;
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarOcorrencia([FromBody] CriarOcorrenciaRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var emailLogado = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == emailLogado);
            if (usuario == null) return Unauthorized();

            var novaOcorrencia = new Ocorrencia
            {
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                TipoOcorrencia = request.TipoOcorrencia,
                DetalhesAdicionais = request.DetalhesAdicionais,
                DataOcorrenciaUtc = DateTime.UtcNow,
                UsuarioId = usuario.Id
            };

            _context.Ocorrencias.Add(novaOcorrencia);
            await _context.SaveChangesAsync();

            var eventoAlerta = new
            {
                Id = novaOcorrencia.Id,
                Tipo = novaOcorrencia.TipoOcorrencia,
                Latitude = novaOcorrencia.Latitude,
                Longitude = novaOcorrencia.Longitude,
                DataUtc = novaOcorrencia.DataOcorrenciaUtc,
                ReportadoPor = usuario.Nome,
                ContatoAutor = usuario.Email
            };

            await _messageBus.PublicarAlertaIncendioAsync(eventoAlerta);

            return StatusCode(201, new { message = "Ocorrência registada e alerta emitido com sucesso", id = novaOcorrencia.Id });
        }

        [HttpGet]
        public async Task<IActionResult> ListarOcorrencias()
        {
            var ocorrencias = await _context.Ocorrencias
                                            .Include(o => o.Usuario)
                                            .ToListAsync();

            var response = ocorrencias.Select(o => new OcorrenciaResponseDto
            {
                Id = o.Id.ToString(),
                Latitude = o.Latitude,
                Longitude = o.Longitude,
                Title = o.TipoOcorrencia,
                Description = o.DetalhesAdicionais ?? "Sem detalhes",
                ReportedAt = o.DataOcorrenciaUtc,
                Urgency = "critical",
                ReportedBy = o.Usuario != null ? o.Usuario.Nome : "Usuário Anônimo"
            });

            return Ok(response);
        }
    }
}