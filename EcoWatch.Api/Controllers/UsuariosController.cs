using EcoWatch.Api.DTOs.Requests;
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
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("meu-perfil")]
        public async Task<IActionResult> ObterPerfil()
        {
            var emailLogado = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(emailLogado)) return Unauthorized();

            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == emailLogado);

            if (usuario == null) return NotFound(new { message = "Usuário não encontrado." });

            var totalReportes = await _context.Ocorrencias
                .CountAsync(o => o.UsuarioId == usuario.Id);

            var response = new PerfilResponseDto
            {
                Nome = usuario.Nome,
                Email = usuario.Email,
                Localidade = usuario.Localidade,
                RaioAlertasKm = usuario.RaioAlertasKm,
                TotalReportes = totalReportes
            };

            return Ok(response);
        }

        [HttpPut("editar-perfil")]
        public async Task<IActionResult> EditarPerfil([FromBody] EditarPerfilRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var emailLogado = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(emailLogado)) return Unauthorized();

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == emailLogado);
            if (usuario == null) return NotFound(new { message = "Usuário não encontrado." });

            usuario.Nome = request.Nome;
            usuario.Localidade = request.Localidade;
            usuario.RaioAlertasKm = request.RaioAlertasKm;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Perfil atualizado com sucesso." });
        }

        [HttpDelete("deletar-conta")]
        public async Task<IActionResult> DeletarConta()
        {
            var emailLogado = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuario = await _context.Usuarios
                .Include(u => u.Ocorrencias)
                .FirstOrDefaultAsync(u => u.Email == emailLogado);

            if (usuario == null) return NotFound();

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}