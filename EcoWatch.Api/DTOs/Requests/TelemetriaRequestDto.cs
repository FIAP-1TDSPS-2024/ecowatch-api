using System.ComponentModel.DataAnnotations;

namespace EcoWatch.Api.DTOs.Requests
{
    public class TelemetriaRequestDto
    {
        [Required(ErrorMessage = "A latitude é obrigatória.")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "A longitude é obrigatória.")]
        public double Longitude { get; set; }

        [Required(ErrorMessage = "A imagem em formato Base64 é obrigatória.")]
        public string ImagemBase64 { get; set; }
    }
}