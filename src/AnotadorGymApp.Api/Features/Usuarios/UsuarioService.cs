using AnotadorGymApp.Api.Features.Usuarios;
using AnotadorGymApp.Api.Features.Usuarios.DTO;
using AnotadorGymApp.Api.Features.Usuarios.Results;
using AnotadorGymAppApi.Domain.Entities.Usuario;
using AnotadorGymAppApi.Features.Usuarios.DTO;
using AnotadorGymAppApi.Infrastructure.Context;
using AnotadorGymAppApi.Infrastructure.Security;
using Azure.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AnotadorGymAppApi.Features.Usuarios
{
    public class UsuarioService : IUsuarioService
    {        
        private readonly IJwtProvider _jwtProvider;        
        private readonly AppDbContext _appDbContext;
        public UsuarioService(IJwtProvider jwtProvider, AppDbContext appDbContext)
        {
            _jwtProvider = jwtProvider;            
            _appDbContext = appDbContext;
        }        
        public async Task<Usuario?> ObtenerUsuarioInvitado()
        {
            var usuarioInvitado = await _appDbContext.Usuarios.
                FirstOrDefaultAsync(u => u.Rol == "Invitado");
            
            return usuarioInvitado;
        }
        public async Task<AuthResult> RegistrarUsuario(RegistroRequestDto nuevoUsuarioDto)
        {
            var authResult = new AuthResult();                  

            // Validar que el usuario (email/username) no exista
            var existeUsuario = await ExisteUsuario(nuevoUsuarioDto.UserName, nuevoUsuarioDto.Email);

            if (existeUsuario)
            {
                authResult.Success = false; 
                authResult.Message = "El usuario ya existe";
                authResult.Error = AuthError.UsuarioYaExiste;
                return authResult;
            }

            // Hashear la contraseña
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(nuevoUsuarioDto.Password);


            // Crear entidad Usuario
            var nuevoUsuario = new Usuario
            {
                UserName = nuevoUsuarioDto.UserName,
                PasswordHash = hashedPassword,
                Email = nuevoUsuarioDto.Email,
                Rol = "Usuario"
            };
            try
            {
                await _appDbContext.Usuarios.AddAsync(nuevoUsuario);
                await _appDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                authResult.Success = false;
                authResult.Message = ex.Message;
                return authResult;
            }

            authResult.Success = true;
            authResult.Message = "Usuario registrado exitosamente";
            authResult.UserName = nuevoUsuario.UserName;

            return authResult;
        }
        public async Task<AuthResult> LoginUsuario(LoginRequestDto request)
        {
            var authResult = new AuthResult();                        

            var existeUsuario = await ObtenerUsuario(request);

            if (existeUsuario == null)
            {                
                authResult.Success = false;
                authResult.Message = "Usuario no encontrado";
                authResult.Error = AuthError.UsuarioNoExiste;
                return authResult;
            }
                        
            // Validar la contraseña
            bool isPasswordValid = ValidarPassword(request.Password, existeUsuario.PasswordHash);
            if (!isPasswordValid)
            {
                authResult.Success = false;
                authResult.Message = "Contraseña incorrecta";
                authResult.Error = AuthError.ContraseñaIncorrecta;
                return authResult;
            }
            

            // Generar token JWT
            var token = _jwtProvider.GenerarJwtToken(existeUsuario);
            authResult.Token = token;
            authResult.Success = true;
            authResult.Message = "Login exitoso";
            authResult.UserName = existeUsuario.UserName;

            return authResult;
        }        

        // Método privado para obtener un usuario por UserName
        private async Task<Usuario?> ObtenerUsuario(LoginRequestDto request)
        {            
            // Normalizar entrada y buscar por userName o email (case-insensitive)
            if (!string.IsNullOrWhiteSpace(request.UserName))
            {
                var n = request.UserName.ToLower();
                return await _appDbContext.Usuarios
                    .FirstOrDefaultAsync(u => u.UserName.ToLower() == n || u.Email.ToLower() == n);
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var e = request.Email.ToLower();
                return await _appDbContext.Usuarios
                    .FirstOrDefaultAsync(u => u.UserName.ToLower() == e || u.Email.ToLower() == e);
            }

            return null;
        }
        
        // Método privado para validar si el usuario ya existe por UserName o Email
        private async Task<bool> ExisteUsuario(string userName, string email)
        {
            var normalizedUser = (userName ?? string.Empty).ToLower();
            var normalizedEmail = (email ?? string.Empty).ToLower();

            return await _appDbContext.Usuarios.AnyAsync(u =>
                u.UserName.ToLower() == normalizedUser ||
                u.Email.ToLower() == normalizedEmail);
        }

        //Metodo privado para validar si la contraseña es correcta
        private bool ValidarPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                return false;
            }
        }
    }
} 
