using EcoWatch.Api.Controllers;
using EcoWatch.Api.DTOs.Requests;
using EcoWatch.Domain.Entities;
using EcoWatch.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EcoWatch.Tests.Api.Controllers
{
    public class AuthControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c["Jwt:Key"]).Returns("UmaChaveMuitoSecretaParaTestesDeUnidadeCom32Caracteres!");

            _controller = new AuthController(_configurationMock.Object, _context);
        }

        [Fact]
        public async Task Registrar_DeveRetornarConflict_QuandoEmailJaEstiverEmUso()
        {
            // Arrange
            var emailExistente = "teste@veloon.com.br";
            _context.Usuarios.Add(new Usuario { Email = emailExistente, Nome = "Teste", SenhaHash = "qualquerhash" });
            await _context.SaveChangesAsync();

            var request = new RegistrarUsuarioRequestDto
            {
                Nome = "Novo Teste",
                Email = emailExistente,
                Senha = "SenhaForte123"
            };

            // Act
            var result = await _controller.Registrar(request);

            // Assert
            var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
            conflictResult.StatusCode.Should().Be(409);
        }

        [Fact]
        public async Task Login_DeveRetornarUnauthorized_QuandoSenhaEstiverIncorreta()
        {
            // Arrange
            var email = "admin@eco.com";
            var senhaReal = "SenhaCorreta123";
            var senhaHash = BCrypt.Net.BCrypt.HashPassword(senhaReal);

            _context.Usuarios.Add(new Usuario { Email = email, Nome = "Admin", SenhaHash = senhaHash });
            await _context.SaveChangesAsync();

            var request = new LoginRequestDto
            {
                Email = email,
                Senha = "SenhaIncorreta456"
            };

            // Act
            var result = await _controller.Login(request);

            // Assert
            var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorizedResult.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Login_DeveRetornarOkComToken_QuandoCredenciaisForemValidas()
        {
            // Arrange
            var email = "valido@eco.com";
            var senhaReal = "MinhaSenhaSuperSecreta";
            var senhaHash = BCrypt.Net.BCrypt.HashPassword(senhaReal);

            _context.Usuarios.Add(new Usuario { Email = email, Nome = "Usuário Válido", SenhaHash = senhaHash });
            await _context.SaveChangesAsync();

            var request = new LoginRequestDto
            {
                Email = email,
                Senha = senhaReal
            };

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);

            var value = okResult.Value;
            value.Should().NotBeNull();
            value!.GetType().GetProperty("Token")?.GetValue(value).Should().NotBeNull();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}