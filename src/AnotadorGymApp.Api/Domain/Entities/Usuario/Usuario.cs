namespace AnotadorGymAppApi.Domain.Entities.Usuario
{
    public class Usuario
    {
        public int UsuarioId { get; set; }
        public string UserName {  get; set; }
        public string Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? GoogleId { get; set; }
        public string Rol {  get; set; }

    }
}
