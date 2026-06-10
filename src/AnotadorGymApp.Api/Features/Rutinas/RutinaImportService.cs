using AnotadorGymAppApi.Domain.Entities.Rutina;
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

            if (rutinasImport == null || !rutinasImport.Any())
            {
                resultado.FalloCritico = true;
                resultado.Errores.Add(new ImportErrorDTO { Mensaje = "Lista de rutinas vacía o nula" });
                _logger.LogWarning("Importación recibida con lista de rutinas nula o vacía.");
                return resultado;
            }

            // Extraemos nombres de ejercicios y normalizamos a minúsculas para comparación insensible a mayúsculas
            var nombreEjercicios = rutinasImport
                .SelectMany(r => r.Semanas ?? Enumerable.Empty<RutinaSemanaDto>())
                .SelectMany(s => s.Dias ?? Enumerable.Empty<RutinaDiaDto>())
                .SelectMany(d => d.Ejercicios ?? Enumerable.Empty<RutinaEjercicioDto>())
                .Select(e => e.Ejercicio?.Nombre)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n!.Trim())
                .Distinct()
                .ToList();

            var nombreEjerciciosNormalized = nombreEjercicios
                .Select(n => n.ToLowerInvariant())
                .ToList();

            // Consultamos ejercicios existentes en la base de datos y construimos un diccionario en memoria
            // para evitar problemas de traducción de expresiones y garantizar búsquedas case-insensitive.
            var ejerciciosEnDb = await appDbContext.Ejercicios.ToListAsync();
            var ejerciciosExistentes = ejerciciosEnDb
                .Where(e => !string.IsNullOrWhiteSpace(e.Nombre))
                .ToDictionary(e => e.Nombre.Trim().ToLowerInvariant(), e => e.EjercicioId);

            // Logging diagnóstico
            _logger.LogDebug("Nombres de ejercicios extraídos del JSON: {Count}", nombreEjercicios.Count);
            _logger.LogDebug("Ejercicios existentes en DB: {Count}", ejerciciosExistentes.Count);

            var errores = new List<ImportErrorDTO>();

            // Comprobamos ejercicios no encontrados
            foreach (var nombre in nombreEjercicios)
            {
                var key = nombre.Trim().ToLowerInvariant();
                if (!ejerciciosExistentes.ContainsKey(key))
                {
                    errores.Add(new ImportErrorDTO
                    {
                        NombreEjercicio = nombre,
                        Mensaje = "Ejercicio no encontrado en la base de datos"
                    });
                }
            }

            // Comprobamos si ya existen rutinas con el mismo nombre (evitar duplicados accidentales)
            var nombresRutinasEntrantes = rutinasImport.Select(r => r.Nombre).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var rutinasExistentesNombres = await appDbContext.Rutinas
                .Where(r => nombresRutinasEntrantes.Contains(r.Nombre))
                .Select(r => r.Nombre)
                .ToListAsync();

            if (rutinasExistentesNombres.Any())
            {
                foreach (var n in rutinasExistentesNombres)
                {
                    errores.Add(new ImportErrorDTO
                    {
                        Mensaje = $"Ya existe una rutina con el nombre: {n}"
                    });
                }
            }

            if (errores.Any())
            {
                resultado.Errores = errores;
                resultado.FalloCritico = true; // No persistimos si hay errores previos
                _logger.LogWarning("Errores detectados antes de persistir: {Count}", errores.Count);
                _logger.LogDebug("Listado de errores:");
                foreach (var er in errores)
                {
                    _logger.LogDebug("- {Mensaje} (Ejercicio: {Ejercicio})", er.Mensaje, er.NombreEjercicio);
                }
                return resultado;
            }

            //Guardamos las rutinas, semanas, dias, ejercicios y series dentro de una transacción
            var rutinasEntidad = new List<Rutina>();
            Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? transaction = null;
            try
            {
                // Detectamos el proveedor de base de datos. Para el proveedor InMemory (usado en tests)
                // no se debe forzar el uso de transacciones ya que no las soporta.
                var providerName = appDbContext.Database.ProviderName ?? string.Empty;
                if (providerName.Contains("InMemory", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Proveedor de BD InMemory detectado ('{Provider}'): se omite la creación de transacción. Configure ConfigureWarnings en el DbContext para suprimir TransactionIgnoredWarning si desea evitar logs.", providerName);
                    transaction = null;
                }
                else
                {
                    // En entornos reales intentamos iniciar transacción y dejamos que cualquier excepción relevante se propague
                    transaction = await appDbContext.Database.BeginTransactionAsync();
                }

                foreach (var rutinaDto in rutinasImport)
                {

                var rutina = new Rutina
                {
                    Nombre = rutinaDto.Nombre,
                    Descripcion = rutinaDto.Descripcion,
                    FrecuenciaPorGrupo = rutinaDto.FrecuenciaPorGrupo ?? string.Empty,
                    Dificultad = rutinaDto.Dificultad ?? string.Empty,
                    TiempoPorSesion = rutinaDto.TiempoPorSesion ?? string.Empty,
                    ImageSource = rutinaDto.ImageSource ?? string.Empty,
                    Semanas = new List<RutinaSemana>()
                };

                    int numSemana = 1;
                    foreach (var semanaDto in rutinaDto.Semanas ?? Enumerable.Empty<RutinaSemanaDto>())
                    {
                        var rutinaSemana = new RutinaSemana
                        {
                            Dias = new List<RutinaDia>(),
                            NumeroSemana = numSemana++
                        };
                        int numDia = 1;

                        foreach (var diaDto in semanaDto.Dias ?? Enumerable.Empty<RutinaDiaDto>())
                        {
                            var rutinaDia = new RutinaDia
                            {
                                Ejercicios = new List<RutinaEjercicio>(),
                                NumeroDia = numDia++
                            };
                            int numEjercicio = 1;

                            foreach (var ejDto in diaDto.Ejercicios ?? Enumerable.Empty<RutinaEjercicioDto>())
                            {
                                var nombreEj = ejDto.Ejercicio?.Nombre?.Trim() ?? string.Empty;
                                var ejercicioKey = nombreEj.ToLowerInvariant();
                                if (!ejerciciosExistentes.TryGetValue(ejercicioKey, out var ejercicioId))
                                {
                                    errores.Add(new ImportErrorDTO
                                    {
                                        NombreEjercicio = nombreEj,
                                        Mensaje = "Ejercicio no encontrado en la base de datos (durante mapeo)"
                                    });
                                    continue;
                                }
                                var rutinaEjercicio = new RutinaEjercicio
                                {
                                    EjercicioId = ejercicioId,
                                    NumeroEjercicio = numEjercicio++,
                                    Series = new List<RutinaSerie>()
                                };
                                int numSerie = 1;

                                foreach (var serieDto in ejDto.Series ?? Enumerable.Empty<RutinaSerieDto>())
                                {
                                    if (!TimeSpan.TryParse(serieDto.Descanso, out var descanso))
                                    {
                                        errores.Add(new ImportErrorDTO
                                        {
                                            NombreEjercicio = nombreEj,
                                            Mensaje = $"Formato de descanso inválido: {serieDto.Descanso}"
                                        });
                                        continue;
                                    }

                                    var serie = new RutinaSerie
                                    {
                                        Repeticiones = serieDto.Repeticiones,
                                        Porcentaje1RM = serieDto.Porcentaje1RM,
                                        Tipo = serieDto.Tipo,
                                        Descanso = descanso,
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

                // Si durante el mapeo surgieron errores (por ejemplo formato de descanso) no persistimos
                if (errores.Any())
                {
                    if (transaction != null)
                    {
                        await transaction.RollbackAsync();
                    }
                    resultado.Errores = errores;
                    resultado.FalloCritico = true;
                    _logger.LogWarning("Errores detectados durante el mapeo, no se persisten cambios. Count={Count}", errores.Count);
                    return resultado;
                }

                await appDbContext.SaveChangesAsync();
                if (transaction != null)
                {
                    await transaction.CommitAsync();
                }

                resultado.RutinasCreadas = rutinasEntidad.Count;
                _logger.LogInformation("Importación finalizada. Rutinas creadas: {Count}", resultado.RutinasCreadas);
                return resultado;
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }
                _logger.LogError(ex, "Error al persistir rutinas importadas");
                resultado.FalloCritico = true;
                resultado.Errores.Add(new ImportErrorDTO { Mensaje = ex.Message, StackTrace = ex.StackTrace });
                return resultado;
            }
        }
    }
}
