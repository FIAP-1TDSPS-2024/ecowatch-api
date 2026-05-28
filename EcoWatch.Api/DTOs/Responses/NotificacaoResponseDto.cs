using System;

namespace EcoWatch.Api.DTOs.Responses
{
    public class NotificacaoResponseDto
    {
        public string IdOcorrencia { get; set; }
        public string Titulo { get; set; }
        public string Mensagem { get; set; }
        public double DistanciaKm { get; set; }
        public string TempoAtras { get; set; }
        public string NivelUrgencia { get; set; }
        public bool Lida { get; set; }
    }
}