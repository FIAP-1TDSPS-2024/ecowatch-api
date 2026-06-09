using EcoWatch.Api.DTOs.Requests;
using EcoWatch.Api.DTOs.Responses;
using EcoWatch.Domain.Entities;
using EcoWatch.Infrastructure.Data;
using EcoWatch.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EcoWatch.Api.Filters;
using System;

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
                UsuarioId = usuario.Id,
                Urgencia = request.Urgencia,
                Area = request.Area,
                Distancia = request.Distancia
            };

            _context.Ocorrencias.Add(novaOcorrencia);
            await _context.SaveChangesAsync();

            var eventoAlerta = new
            {
                Id = novaOcorrencia.Id,
                Tipo = novaOcorrencia.TipoOcorrencia,
                Latitude = novaOcorrencia.Latitude,
                Longitude = novaOcorrencia.Longitude,
                Urgencia = novaOcorrencia.Urgencia,
                Area = novaOcorrencia.Area,
                Distancia = novaOcorrencia.Distancia,
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
                Urgency = o.Urgencia ?? "baixa",
                Area = o.Area ?? 0.0,
                Distance = o.Distancia ?? 0.0,
                ReportedBy = o.Usuario != null ? o.Usuario.Nome : "Usuário Anônimo"
            });

            return Ok(response);
        }

        [HttpPost("satelite")]
        [AllowAnonymous]
        [ApiKeyAuth]
        public async Task<IActionResult> RegistrarOcorrenciaViaSatelite([FromBody] AlertaSateliteRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var emailSatelite = "ia-satelite@ecowatch.com";
            var botUsuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == emailSatelite);

            if (botUsuario == null)
            {
                botUsuario = new Usuario
                {
                    Id = Guid.NewGuid(),
                    Nome = "Sistema de Detecção (IA Satélite)",
                    Email = emailSatelite,
                    SenhaHash = "SERVICE_ACCOUNT_NO_LOGIN"
                };

                _context.Usuarios.Add(botUsuario);
                await _context.SaveChangesAsync();
            }

            var novaOcorrencia = new Ocorrencia
            {
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                TipoOcorrencia = "Incêndio Florestal (Detecção IA)",
                DetalhesAdicionais = $"Foco validado por Visão Computacional com {Math.Round(request.Confianca * 100, 2)}% de confiança. Ref Mongo: {request.ReferenciaIdMongo}",
                DataOcorrenciaUtc = DateTime.UtcNow,
                Urgencia = request.Confianca > 0.85 ? "Crítica" : "Alta",
                UsuarioId = botUsuario.Id
            };

            _context.Ocorrencias.Add(novaOcorrencia);
            await _context.SaveChangesAsync();

            var eventoAlerta = new
            {
                Id = novaOcorrencia.Id,
                Tipo = novaOcorrencia.TipoOcorrencia,
                Latitude = novaOcorrencia.Latitude,
                Longitude = novaOcorrencia.Longitude,
                Urgencia = novaOcorrencia.Urgencia,
                DataUtc = novaOcorrencia.DataOcorrenciaUtc,
                ReportadoPor = botUsuario.Nome
            };

            await _messageBus.PublicarAlertaIncendioAsync(eventoAlerta);

            return StatusCode(201, new { message = "Alerta de satélite integrado com sucesso.", id = novaOcorrencia.Id });
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> AtualizarOcorrencia(Guid id, [FromBody] AtualizarOcorrenciaRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var emailLogado = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuarioLogado = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == emailLogado);
            if (usuarioLogado == null) return Unauthorized();

            var ocorrencia = await _context.Ocorrencias.FirstOrDefaultAsync(o => o.Id == id);

            if (ocorrencia == null)
                return NotFound(new { message = "Ocorrência não encontrada." });

            if (ocorrencia.UsuarioId != usuarioLogado.Id)
                return Forbid();

            ocorrencia.Latitude = request.Latitude;
            ocorrencia.Longitude = request.Longitude;
            ocorrencia.TipoOcorrencia = request.TipoOcorrencia;
            ocorrencia.DetalhesAdicionais = request.DetalhesAdicionais;
            ocorrencia.Urgencia = request.Urgencia;
            ocorrencia.Area = request.Area;
            ocorrencia.Distancia = request.Distancia;

            _context.Ocorrencias.Update(ocorrencia);
            await _context.SaveChangesAsync();

            return Ok(new OcorrenciaResponseDto
            {
                Id = ocorrencia.Id.ToString(),
                Latitude = ocorrencia.Latitude,
                Longitude = ocorrencia.Longitude,
                Title = ocorrencia.TipoOcorrencia,
                Description = ocorrencia.DetalhesAdicionais ?? "Sem detalhes",
                ReportedAt = ocorrencia.DataOcorrenciaUtc,
                Urgency = ocorrencia.Urgencia ?? "baixa",
                Area = ocorrencia.Area ?? 0.0,
                Distance = ocorrencia.Distancia ?? 0.0,
                ReportedBy = usuarioLogado.Nome
            });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeletarOcorrencia(Guid id)
        {
            var emailLogado = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuarioLogado = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == emailLogado);
            if (usuarioLogado == null) return Unauthorized();

            var ocorrencia = await _context.Ocorrencias.FirstOrDefaultAsync(o => o.Id == id);

            if (ocorrencia == null)
                return NotFound(new { message = "Ocorrência não encontrada." });

            if (ocorrencia.UsuarioId != usuarioLogado.Id)
            {
                return Forbid();
            }

            _context.Ocorrencias.Remove(ocorrencia);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}