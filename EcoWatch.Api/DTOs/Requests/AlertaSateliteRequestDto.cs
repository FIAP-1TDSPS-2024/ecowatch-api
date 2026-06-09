using System.ComponentModel.DataAnnotations;

namespace EcoWatch.Api.DTOs.Requests
{
    public class AlertaSateliteRequestDto
    {
        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required]
        public double Confianca { get; set; }

        [Required]
        public string ReferenciaIdMongo { get; set; }
    }
}