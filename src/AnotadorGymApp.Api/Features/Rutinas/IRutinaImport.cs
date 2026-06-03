using AnotadorGymAppApi.Features.Common.Results;
using AnotadorGymAppApi.Features.Rutinas.DTOs;

namespace AnotadorGymAppApi.Features.Rutinas
{
    public interface IRutinaImport
    {
        Task<ImportResultDTO> ImportarRutinasDesdeJsonAsync(List<RutinaDto> rutinasImport);
        Task<ImportResultDTO> ImportarRutinaDesdeArchivoAsync(IFormFile archivo);
    }
}
