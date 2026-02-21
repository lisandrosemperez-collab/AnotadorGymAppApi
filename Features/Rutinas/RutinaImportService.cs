using AnotadorGymAppApi.Domain.Entities;
using AnotadorGymAppApi.Features.Common.Results;
using AnotadorGymAppApi.Features.Common.Validation;
using AnotadorGymAppApi.Features.Rutinas.DTOs;
using AnotadorGymAppApi.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AnotadorGymAppApi.Features.Rutinas
{
    public class RutinaImportService : IRutinaImport
    {
        private readonly AppDbContext appDbContext;
        private readonly IJsonFileValidator _jsonFileValidator;
        private readonly ILogger<RutinaImportService> _logger;
        public RutinaImportService(AppDbContext appDbContext, ILogger<RutinaImportService> logger,IJsonFileValidator jsonFileValidator)
        {
            this.appDbContext = appDbContext;
            _logger = logger;
            _jsonFileValidator = jsonFileValidator;
        }
        public async Task<ImportResultDTO> ImportarRutinaDesdeArchivoAsync(IFormFile archivo)
        {
            _logger.LogInformation($"Iniciando importación desde archivo: {archivo.FileName}");
            var inicio = DateTime.UtcNow;
            var resultado = new ImportResultDTO();

            try
            {
                var datos = await _jsonFileValidator.ValidateJsonFileAsync<List<RutinaDto>>(archivo);

                if (!datos.esValido)
                {
                    _logger.LogWarning(datos.Error);
                    resultado = new ImportResultDTO
                    {
                        Duracion = DateTime.UtcNow - inicio,                           
                        FalloCritico = true
                    };
                    resultado.Errores.Add(new ImportErrorDTO
                    {
                        Mensaje = $"Error al procesar archivo: {datos.Error}",                        
                    });
                    return resultado;
                }

                resultado = await ImportarRutinasDesdeJsonAsync(datos.Data);
                resultado.Duracion = DateTime.UtcNow - inicio;

                return resultado;

            }catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar archivo JSON");
                
                resultado = new ImportResultDTO
                {
                    Duracion = DateTime.UtcNow - inicio,
                    RutinasCreadas = 0,
                    FalloCritico = true
                };
                resultado.Errores.Add(new ImportErrorDTO
                {
                    Mensaje = $"Error al procesar archivo: {ex.Message}",
                    StackTrace = ex.StackTrace
                });

                return resultado;
            }
        }
        public async Task<ImportResultDTO> ImportarRutinasDesdeJsonAsync(List<RutinaDto> rutinasImport)
        {
            var resultado = new ImportResultDTO();

            var nombreEjercicios = rutinasImport
                .SelectMany(r => r.Semanas)
                .SelectMany(s => s.Dias)
                .SelectMany(d => d.Ejercicios)
                .Select(e => e.Ejercicio.Nombre)
                .Distinct()
                .ToList();

            var ejerciciosExistentes = await appDbContext.Ejercicios
                .Where(e => nombreEjercicios.Contains(e.Nombre))
                .ToDictionaryAsync(e => e.Nombre, e => e.EjercicioId);

            var errores = new List<ImportErrorDTO>();

            foreach (var nombre in nombreEjercicios)
            {
                if (!ejerciciosExistentes.ContainsKey(nombre))
                {
                    errores.Add(new ImportErrorDTO
                    {
                        NombreEjercicio = nombre,
                        Mensaje = "Ejercicio no encontrado en la base de datos"
                    });
                }
            }

            if (errores.Any())
            {
                resultado.Errores = errores;
                return resultado;
            }

            //Guardamos las rutinas, semanas, dias, ejercicios y series
            var rutinasEntidad = new List<Rutina>();
            foreach (var rutinaDto in rutinasImport)
            {
                var rutina = new Rutina
                {
                    Nombre = rutinaDto.Nombre,
                    Descripcion = rutinaDto.Descripcion,
                    FrecuenciaPorGrupo = rutinaDto.FrecuenciaPorGrupo,
                    Dificultad = rutinaDto.Dificultad,
                    TiempoPorSesion = rutinaDto.TiempoPorSesion,
                    ImageSource = rutinaDto.ImageSource,
                    Semanas = new List<RutinaSemana>()
                };

                int numSemana = 1;
                foreach (var semanaDto in rutinaDto.Semanas)
                {
                    var rutinaSemana = new RutinaSemana
                    {
                        Dias = new List<RutinaDia>(),
                        NumeroSemana = numSemana++
                    };
                    int numDia = 1;

                    foreach (var diaDto in semanaDto.Dias)
                    {
                        var rutinaDia = new RutinaDia
                        {                                                
                            Ejercicios = new List<RutinaEjercicio>(),
                            NumeroDia = numDia++
                        };
                        int numEjercicio = 1;

                        foreach (var ejDto in diaDto.Ejercicios)
                        {
                            var ejercicioId = ejerciciosExistentes[ejDto.Ejercicio.Nombre];
                            var rutinaEjercicio = new RutinaEjercicio
                            {
                                EjercicioId = ejercicioId,
                                NumeroEjercicio = numEjercicio++,
                                Series = new List<RutinaSerie>()
                            };
                            int numSerie = 1;

                            foreach (var serieDto in ejDto.Series)
                            {
                                if (!TimeSpan.TryParse(serieDto.Descanso, out var descanso))
                                {                                    
                                    errores.Add(new ImportErrorDTO
                                    {
                                        NombreEjercicio = ejDto.Ejercicio.Nombre,
                                        Mensaje = $"Formato de descanso inválido: {serieDto.Descanso}"
                                    });
                                    continue;
                                }

                                var serie = new RutinaSerie
                                {
                                    Repeticiones = serieDto.Repeticiones,
                                    Porcentaje1RM = serieDto.Porcentaje1RM,
                                    Tipo = serieDto.Tipo,
                                    Descanso = TimeSpan.Parse(serieDto.Descanso),
                                    NumeroSerie = numSerie++
                                };
                                rutinaEjercicio.Series.Add(serie);
                            }
                            rutinaDia.Ejercicios.Add(rutinaEjercicio);
                        }
                        rutinaSemana.Dias.Add(rutinaDia);
                    }

                    rutina.Semanas.Add(rutinaSemana);                    
                }   
                rutinasEntidad.Add(rutina);
                appDbContext.Rutinas.Add(rutina);                
            }

            await appDbContext.SaveChangesAsync();            
            resultado.RutinasCreadas = rutinasEntidad.Count;
            return resultado;
        }
    }
}
