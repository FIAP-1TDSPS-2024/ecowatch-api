using System;

namespace EcoWatch.Domain.Entities
{
    public class Ocorrencia
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string TipoOcorrencia { get; set; }
        public string DetalhesAdicionais { get; set; }
        public DateTime DataOcorrenciaUtc { get; set; }
        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
        public string? Urgencia { get; set; }
        public double? Area { get; set; }
        public double? Distancia { get; set; }
    }
}