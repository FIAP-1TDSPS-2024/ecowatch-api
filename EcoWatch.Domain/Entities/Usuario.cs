using System;
using System.Collections.Generic;

namespace EcoWatch.Domain.Entities
{
    public class Usuario
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; }
        public string Email { get; set; }
        public string SenhaHash { get; set; }
        public string Role { get; set; } = "Cidadao";
        public DateTime DataCadastroUtc { get; set; } = DateTime.UtcNow;
        public string? Localidade { get; set; }
        public int RaioAlertasKm { get; set; } = 30;

        public ICollection<Ocorrencia> Ocorrencias { get; set; } = new List<Ocorrencia>();
    }
}