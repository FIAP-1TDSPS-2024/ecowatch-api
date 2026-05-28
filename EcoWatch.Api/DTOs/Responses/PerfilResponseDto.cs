namespace EcoWatch.Api.DTOs.Responses
{
    public class PerfilResponseDto
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Localidade { get; set; }
        public int RaioAlertasKm { get; set; }
        public int TotalReportes { get; set; }
    }
}