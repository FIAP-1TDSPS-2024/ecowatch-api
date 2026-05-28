using EcoWatch.Api.DTOs.Requests;
using EcoWatch.Domain.Entities;
using EcoWatch.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace EcoWatch.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AuthController(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] RegistrarUsuarioRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Email == request.Email);
            if (usuarioExiste)
            {
                return Conflict(new { message = "Este e-mail já está em uso." });
            }

            string senhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);

            var novoUsuario = new Usuario
            {
                Nome = request.Nome,
                Email = request.Email,
                SenhaHash = senhaHash
            };

            _context.Usuarios.Add(novoUsuario);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { message = "Usuário cadastrado com sucesso", id = novoUsuario.Id });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
            {
                return Unauthorized(new { message = "Credenciais inválidas" });
            }

            var token = GerarJwtToken(usuario);
            return Ok(new { Token = token, Nome = usuario.Nome, Email = usuario.Email, Role = usuario.Role });
        }

        private string GerarJwtToken(Usuario usuario)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Role, usuario.Role)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}