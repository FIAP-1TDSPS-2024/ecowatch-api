using EcoWatch.Api.Controllers;
using EcoWatch.Api.DTOs.Requests;
using EcoWatch.Application.Services;
using EcoWatch.Domain.Entities;
using EcoWatch.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace EcoWatch.Tests.Api.Controllers
{
    public class OcorrenciasControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IMessageBusService> _messageBusMock;
        private readonly OcorrenciasController _controller;

        public OcorrenciasControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            _messageBusMock = new Mock<IMessageBusService>();

            _controller = new OcorrenciasController(_context, _messageBusMock.Object);
        }

        private void SimularUsuarioLogado(string email)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, email) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task RegistrarOcorrencia_DeveRetornarUnauthorized_QuandoUsuarioNaoExistirNoBanco()
        {
            // Arrange
            var request = new CriarOcorrenciaRequestDto { Latitude = -23.5, Longitude = -46.6, TipoOcorrencia = "Fogo", Urgencia = "Alta" };
            SimularUsuarioLogado("usuario_fantasma@teste.com");

            // Act
            var result = await _controller.RegistrarOcorrencia(request);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>("porque o banco de dados não contém o usuário do token");
        }

        [Fact]
        public async Task RegistrarOcorrencia_DeveSalvarNoBancoEPublicarNoRabbit_QuandoDadosForemValidos()
        {
            // Arrange
            var emailValido = "jonas.wendell@teste.com";
            var usuario = new Usuario { Email = emailValido, Nome = "Jonas Wendell", SenhaHash = "hash" };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            SimularUsuarioLogado(emailValido);

            var request = new CriarOcorrenciaRequestDto
            {
                Latitude = -23.5333,
                Longitude = -46.7917,
                TipoOcorrencia = "Incêndio Florestal",
                Urgencia = "Crítica",
                DetalhesAdicionais = "Fogo próximo à rodovia",
                Area = 10.5,
                Distancia = 5.0
            };

            // Act
            var result = await _controller.RegistrarOcorrencia(request);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(201);

            var ocorrenciaSalva = await _context.Ocorrencias.FirstOrDefaultAsync();
            ocorrenciaSalva.Should().NotBeNull();
            ocorrenciaSalva!.TipoOcorrencia.Should().Be(request.TipoOcorrencia);
            ocorrenciaSalva.UsuarioId.Should().Be(usuario.Id);

            _messageBusMock.Verify(m => m.PublicarAlertaIncendioAsync(It.Is<object>(obj =>
                obj.GetType().GetProperty("Tipo")!.GetValue(obj)!.ToString() == "Incêndio Florestal" &&
                obj.GetType().GetProperty("ReportadoPor")!.GetValue(obj)!.ToString() == "Jonas Wendell"
            )), Times.Once, "o sistema deve publicar exatamente 1 evento no RabbitMQ ao criar uma ocorrência.");
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}