using AnotadorGymApp.Api.Features.Entrenamiento.DTOs;

namespace AnotadorGymApp.Api.Features.Entrenamiento
{
    public interface IEntrenamientoService
    {
        Task<int> CrearAsync(EntrenamientoDto dto,int usuarioId, CancellationToken cancellationToken);
        Task<EntrenamientoDto?> ObtenerPorIdAsync(int entrenamientoId,int usuarioId, CancellationToken cancellationToken);
        Task<IEnumerable<EntrenamientoDto>> ObtenerPorUsuarioAsync(int usuarioId, CancellationToken cancellationToken);
        Task<bool> SincronizarEntrenamiento(int usuarioId, EntrenamientoDto dto, CancellationToken cancellationToken);
        Task<bool> BorrarAsync(int usuarioId,int entrenamientoId, CancellationToken cancellationToken);
    }
}
