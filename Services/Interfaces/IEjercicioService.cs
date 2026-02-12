using AnotadorGymAppApi.DTOs.Ejercicio;

namespace AnotadorGymAppApi.Services.Interfaces
{
    public interface IEjercicioService
    {
        public Task<IEnumerable<EjercicioDTO>> GetAllEjercicios();
        public Task<EjercicioDTO?> GetEjercicioById(int id);
        public Task<IEnumerable<EjercicioSimpleDTO>> GetEjerciciosByGrupoMuscular(int grupoMuscularId);
        public Task<IEnumerable<EjercicioSimpleDTO>> GetEjerciciosByMusculoPrimario(int musculoPrimarioId);
        public Task<IEnumerable<EjercicioSimpleDTO>> SearchEjercicios(string search);
    }
}
