using AnotadorGymAppApi.Features.Common.Results;
using AnotadorGymAppApi.Features.Ejercicios.DTOs;

namespace AnotadorGymAppApi.Features.Ejercicios
{
    public interface IEjercicioImport
    {
        Task<ImportResultDTO> ImportarEjerciciosDesdeJsonAsync(List<EjercicioDTO> ejerciciosImport);
        Task<ImportResultDTO> ImportarEjerciciosDesdeArchivoAsync(IFormFile archivo);
    }
}
