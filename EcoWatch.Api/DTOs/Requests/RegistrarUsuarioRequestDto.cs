using System.ComponentModel.DataAnnotations;

namespace EcoWatch.Api.DTOs.Requests
{
    public class RegistrarUsuarioRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        public string Senha { get; set; }
    }
}