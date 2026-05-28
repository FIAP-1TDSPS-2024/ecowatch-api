using System.ComponentModel.DataAnnotations;

namespace EcoWatch.Api.DTOs.Requests
{
    public class CriarOcorrenciaRequestDto
    {
        [Required]
        [Range(-90.0, 90.0)]
        public double Latitude { get; set; }

        [Required]
        [Range(-180.0, 180.0)]
        public double Longitude { get; set; }

        [Required]
        [MaxLength(50)]
        public string TipoOcorrencia { get; set; }

        [MaxLength(500)]
        public string DetalhesAdicionais { get; set; }
    }
}