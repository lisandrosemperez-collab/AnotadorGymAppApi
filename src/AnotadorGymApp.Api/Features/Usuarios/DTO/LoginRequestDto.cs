using System.ComponentModel.DataAnnotations;

namespace AnotadorGymApp.Api.Features.Usuarios.DTO
{
    public class LoginRequestDto
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
