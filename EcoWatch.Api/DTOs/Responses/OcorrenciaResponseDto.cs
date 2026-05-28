using System;

namespace EcoWatch.Api.DTOs.Responses
{
    public class OcorrenciaResponseDto
    {
        public string Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Urgency { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime ReportedAt { get; set; }
        public double Area { get; set; }
        public double Distance { get; set; }
        public bool FirefightersDispatched { get; set; }
        public string ReportedBy { get; set; }
    }
}