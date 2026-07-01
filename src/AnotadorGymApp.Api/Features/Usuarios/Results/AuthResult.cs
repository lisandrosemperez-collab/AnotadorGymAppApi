using AnotadorGymAppApi.Features.Usuarios.DTO;

namespace AnotadorGymApp.Api.Features.Usuarios.Results
{
    public class AuthResult
    {
        public AuthError Error { get; set; } = AuthError.Ninguno;
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }

    public enum AuthError
    {
        Ninguno,
        UsuarioYaExiste,
        ContraseñaIncorrecta,
        UsuarioNoExiste,
        EmailNoExiste,
        EmailYaExiste,
        InternalError
    }
}
