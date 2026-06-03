using AnotadorGymAppApi.Features.Common.Results;
using AnotadorGymAppApi.Features.Ejercicios.DTOs;

namespace AnotadorGymAppApi.Features.Ejercicios
{
    public interface IEjercicioImport
    {
        Task<ImportResultDTO> ImportarEjerciciosDesdeJsonAsync(List<EjercicioDto> ejerciciosImport);
        Task<ImportResultDTO> ImportarEjerciciosDesdeArchivoAsync(IFormFile archivo);
    }
}
