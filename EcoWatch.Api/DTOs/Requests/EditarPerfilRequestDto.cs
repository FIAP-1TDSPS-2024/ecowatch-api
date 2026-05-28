using System.ComponentModel.DataAnnotations;

namespace EcoWatch.Api.DTOs.Requests
{
    public class EditarPerfilRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        [MaxLength(100)]
        public string Localidade { get; set; }

        [Range(1, 100)]
        public int RaioAlertasKm { get; set; }
    }
}