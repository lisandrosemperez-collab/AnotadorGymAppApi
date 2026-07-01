using System.ComponentModel.DataAnnotations;

namespace AnotadorGymApp.Api.Features.Usuarios.DTO
{
    public class RegistroRequestDto
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [MaxLength(24)]
        public string Password { get; set; } = string.Empty;        
    }
}
