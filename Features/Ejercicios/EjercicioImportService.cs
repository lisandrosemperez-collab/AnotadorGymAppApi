using AnotadorGymAppApi.Domain.Entities.Ejercicio;
using AnotadorGymAppApi.Features.Common.Results;
using AnotadorGymAppApi.Features.Common.Validation;
using AnotadorGymAppApi.Features.Ejercicios.DTOs;
using AnotadorGymAppApi.Infrastructure.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AnotadorGymAppApi.Features.Ejercicios
{
    public class EjercicioImportService : IEjercicioImport
    {
        private readonly AppDbContext appDbContext;
        private readonly ILogger<EjercicioImportService> _logger;
        private readonly IJsonFileValidator _jsonFileValidator;
        public EjercicioImportService(AppDbContext appDbContext,ILogger<EjercicioImportService> logger, IJsonFileValidator jsonFileValidator)
        {
            this.appDbContext = appDbContext;
            _logger = logger;
            _jsonFileValidator = jsonFileValidator;
        }

        public async Task<ImportResultDTO> ImportarEjerciciosDesdeArchivoAsync(IFormFile archivo)
        {
            _logger.LogInformation($"Iniciando importación desde archivo: {archivo.FileName}");
            var inicio = DateTime.UtcNow;
            var resultado = new ImportResultDTO();

            try
            {
                var datos = await _jsonFileValidator.ValidateJsonFileAsync<List<EjercicioDTO>>(archivo);

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

                resultado = await ImportarEjerciciosDesdeJsonAsync(datos.Data);

                resultado.Duracion = DateTime.UtcNow - inicio;

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar archivo JSON");
                
                resultado = new ImportResultDTO
                {
                    Duracion = DateTime.UtcNow - inicio
                };
                resultado.Errores.Add(new ImportErrorDTO
                {
                    Mensaje = $"Error al procesar archivo: {ex.Message}",
                    StackTrace = ex.StackTrace
                });

                return resultado;
            }
            
        }
        public async Task<ImportResultDTO> ImportarEjerciciosDesdeJsonAsync(List<EjercicioDTO> ejerciciosJson)
        {
            var resultado = new ImportResultDTO();            

            _logger.LogInformation($"Iniciando importación de {ejerciciosJson.Count} ejercicios");

            try
            {
                //Grupos Musculares del JSON
                var gruposMuscularesJson = ejerciciosJson
                    .Where(e => !string.IsNullOrWhiteSpace(e.GrupoMuscular?.Nombre))
                    .Select(e => e.GrupoMuscular!.Nombre.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                //Musculos Especificos del JSON
                var musculosJson = ejerciciosJson
                    .Select(e => e.MusculoPrimario?.Nombre?.Trim())
                    .Concat(ejerciciosJson
                        .SelectMany(e => e.MusculosSecundarios?
                            .Select(m => m.Nombre?.Trim()) ?? Enumerable.Empty<string>()))
                    .Where(m => !string.IsNullOrWhiteSpace(m))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();


                #region //Procesar grupos musculares

                _logger.LogInformation($"Procesando {gruposMuscularesJson.Count} grupos musculares");

                var gruposMuscularesDb = await appDbContext.GrupoMusculares
                    .ToDictionaryAsync(x => Normalizar(x.Nombre), x => x.GrupoMuscularId);

                var gruposMuscularesDict = ProcesarGruposMuscularesAsync(
                    gruposMuscularesJson,
                    resultado,
                    gruposMuscularesDb);

                await appDbContext.SaveChangesAsync();

                
                gruposMuscularesDict = await appDbContext.GrupoMusculares
                    .ToDictionaryAsync(x => x.Nombre.ToLower(), x => x.GrupoMuscularId);

                #endregion

                #region // Procesar musculos

                _logger.LogInformation($"Procesando {musculosJson.Count} músculos");
                
                var musculosDb = await appDbContext.Musculos                    
                    .ToDictionaryAsync(m => Normalizar(m.Nombre), m => m.MusculoId);
                
                var musculosDict = ProcesarMusculosAsync(musculosJson, resultado, musculosDb);

                await appDbContext.SaveChangesAsync();

                musculosDict = await appDbContext.Musculos
                    .ToDictionaryAsync(m => Normalizar(m.Nombre), m => m.MusculoId);

                #endregion

                _logger.LogInformation($"Procesando {ejerciciosJson.Count} ejercicios");                
                
                var strategy = appDbContext.Database.CreateExecutionStrategy();

                // Cargar ejercicios existentes en un diccionario SOLO PARA VALIDACIÓN rápida
                var ejerciciosDb = await appDbContext.Ejercicios                                        
                    .ToDictionaryAsync(e => e.Nombre.ToLower(), e => e);

                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await appDbContext.Database.BeginTransactionAsync();
                    try
                    {
                        await ProcesarEjerciciosAsync(
                            ejerciciosJson, gruposMuscularesDict, musculosDict, resultado, ejerciciosDb);

                        await appDbContext.SaveChangesAsync();

                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Error fatal en la transacción principal");
                        resultado.Errores.Add(new ImportErrorDTO
                        {
                            Mensaje = $"Error fatal: {ex.Message}",
                            StackTrace = ex.StackTrace
                        });
                        throw;
                    }
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la importación de ejercicios");
                resultado.Errores.Add(new ImportErrorDTO
                {
                    Mensaje = $"Error general: {ex.Message}",
                    StackTrace = ex.StackTrace
                });                
            }
                        
            _logger.LogInformation("Importación completada." +
                "Creados: {Creados}, Actualizados: {Actualizados}, Errores: {Errores}",                
                resultado.EjerciciosCreados,
                resultado.EjerciciosActualizados,
                resultado.Errores.Count);            

            return resultado;
        }
        private async Task ProcesarEjerciciosAsync(List<EjercicioDTO> ejerciciosJson, 
            Dictionary<string, int> gruposMuscularesDict, Dictionary<string, int> musculosDict, 
            ImportResultDTO importResult, Dictionary<string, Ejercicio> ejerciciosDb)
        {
            int batchSize = 100;
            int ejerciciosCreadosEnLote = 0;
            int totalProcesados = 0;

            var ejerciciosValidados = new List<(Ejercicio Ejercicio, int IndiceOriginal)>();
            var nombresProcesados = new HashSet<string>();
            var indicesDuplicados = new HashSet<int>();

            var nombresExistentes = new HashSet<string>(
                ejerciciosDb.Keys.Select(Normalizar),
                StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < ejerciciosJson.Count; i++)
            {
                var ejercicioJson = ejerciciosJson[i];
                if (!string.IsNullOrWhiteSpace(ejercicioJson.Nombre))
                {
                    var nombreNormalizado = Normalizar(ejercicioJson.Nombre);

                    if (nombresProcesados.Contains(nombreNormalizado))
                    {
                        indicesDuplicados.Add(i);
                        _logger.LogWarning($"Ejercicio duplicado en JSON en índice {i}: {ejercicioJson.Nombre}");
                    }
                    else
                    {
                        nombresProcesados.Add(nombreNormalizado);
                    }
                }
            }

            for (int i = 0; i < ejerciciosJson.Count; i++)
            {

                totalProcesados ++;
                if (indicesDuplicados.Contains(i))
                {
                    importResult.Advertencias.Add($"Ejercicio duplicado omitido: {ejerciciosJson[i].Nombre}");
                    continue;
                }

                try
                {
                    var ejercicioJson = ejerciciosJson[i];

                    #region Validaciones
                    if (!ValidarEjercicioImport(ejercicioJson, i, importResult))
                        continue;

                    var nombreNormalizado = Normalizar(ejercicioJson.Nombre);

                    // Buscar grupo muscular
                    if (!gruposMuscularesDict.TryGetValue(Normalizar(ejercicioJson.GrupoMuscular.Nombre!),
                        out var grupoMuscularId))
                    {
                        AgregarError(i, ejercicioJson.Nombre,
                            $"Grupo muscular '{ejercicioJson.GrupoMuscular}' no encontrado",
                            importResult);
                        continue;
                    }

                    // Buscar musculo primario
                    if (!musculosDict.TryGetValue(Normalizar(ejercicioJson.MusculoPrimario!.Nombre!),
                        out var musculoPrimarioId))
                    {
                        AgregarError(i, ejercicioJson.Nombre,
                            $"Músculo primario '{ejercicioJson.MusculoPrimario.Nombre}' no encontrado",
                            importResult);
                        continue;
                    }
                    // Verificar si ya existe en BD (validación rápida con diccionario)                    

                    if (nombresExistentes.Contains(nombreNormalizado))
                    {
                        importResult.EjerciciosOmitidos++;
                        _logger.LogDebug($"Ejercicio {ejercicioJson.Nombre} ya existe, se omite");
                        
                        continue;
                    }
                    #endregion

                    // Crear nuevo ejercicio
                    var nuevoEjercicio = CrearNuevoEjercicioAsync(
                        ejercicioJson, grupoMuscularId, musculoPrimarioId,
                        musculosDict);                    

                    nombresExistentes.Add(nombreNormalizado);

                    ejerciciosValidados.Add((nuevoEjercicio, i));                                                            

                    _logger.LogDebug($"Ejercicio {ejercicioJson.Nombre} pasó todas las validaciones");
                    
                }
                catch (Exception ex)
                {
                    // El error ya fue registrado en ValidarEjercicioAntesDeGuardarAsync
                    _logger.LogDebug($"Ejercicio en índice {i} falló validación: {ex.Message}");
                }
            }

            _logger.LogInformation($"Validación completada. {totalProcesados} ejercicios procesados, {ejerciciosValidados.Count} pasaron validación");

            for (int batchIndex = 0; batchIndex < ejerciciosValidados.Count; batchIndex += batchSize)
            {
                var batch = ejerciciosValidados.Skip(batchIndex).Take(batchSize).ToList();

                try
                {
                    // Agregar todos los ejercicios del batch al contexto
                    foreach (var (ejercicio, indiceOriginal) in batch)
                    {
                        ejercicio.EjercicioId = 0;
                        appDbContext.Ejercicios.Add(ejercicio);
                    }                    

                    // ✅ INTENTAR GUARDAR EL BATCH
                    _logger.LogInformation($"Guardando batch {batchIndex / batchSize + 1} con {batch.Count} ejercicios...");

                    var duplicadosEnBatch = batch
                        .GroupBy(x => Normalizar(x.Ejercicio.Nombre))
                        .Where(g => g.Count() > 1)
                        .ToList();

                    foreach (var dup in duplicadosEnBatch)
                    {
                        _logger.LogError($"DUPLICADO EN BATCH: {dup.Key} ({dup.Count()} veces)");
                    }

                    await appDbContext.SaveChangesAsync();

                    appDbContext.ChangeTracker.Clear();

                    ejerciciosCreadosEnLote += batch.Count;
                    importResult.EjerciciosCreados += batch.Count;

                    _logger.LogInformation(
                        $"Batch {batchIndex / batchSize + 1} guardado exitosamente: {batch.Count} ejercicios creados");
                }
                catch (DbUpdateException dbEx)
                {
                    // ✅ ¡ERROR EN EL BATCH! Ahora guardamos uno por uno para identificar el problema
                    _logger.LogWarning(dbEx,$"Error al guardar batch {batchIndex / batchSize + 1}. Guardando ejercicios individualmente...");                    

                    await GuardarEjerciciosIndividualmenteAsync(
                        batch, importResult);
                }
            }            
        }
        private async Task GuardarEjerciciosIndividualmenteAsync(
                        List<(Ejercicio Ejercicio, int IndiceOriginal)> batch,
                        ImportResultDTO resultado)
        {
            foreach (var (ejercicio, indiceOriginal) in batch)
            {                
                try
                {
                    // Antes de guardar, validamos nuevamente el ejercicio para asegurarnos de que no haya problemas de integridad referencial o duplicados
                    appDbContext.ChangeTracker.Clear();
                    ejercicio.EjercicioId = 0;

                    // Volver a agregar solo este ejercicio
                    appDbContext.Ejercicios.Add(ejercicio);

                    // ✅ GUARDAR SOLO ESTE EJERCICIO
                    await appDbContext.SaveChangesAsync();                    

                    resultado.EjerciciosCreados++;

                    _logger.LogDebug($"Ejercicio creado individualmente: {ejercicio.Nombre}");
                }
                catch (DbUpdateException dbEx)
                {                    
                    var mensajeError = ObtenerMensajeErrorDb(dbEx);

                    _logger.LogError(dbEx,
                        $"Error al guardar ejercicio individualmente en índice {indiceOriginal}: {ejercicio.Nombre} - {mensajeError}");

                    AgregarError(indiceOriginal, ejercicio.Nombre,
                        $"Error de base de datos al guardar: {mensajeError}",
                        resultado, dbEx.StackTrace);                    
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,$"Error inesperado al guardar ejercicio individualmente: {ejercicio.Nombre}");

                    AgregarError(indiceOriginal, ejercicio.Nombre,
                        $"Error inesperado: {ex.Message}",
                        resultado, ex.StackTrace);
                }
            }
        }
        private Ejercicio CrearNuevoEjercicioAsync(EjercicioDTO ejercicioJson, int GrupoMuscularId, int MusculoPrimarioId, Dictionary<string, int> musculosDict)
        {
            var nuevoEjercicio = new Ejercicio
            {
                Nombre = ejercicioJson.Nombre,                
                GrupoMuscularId = GrupoMuscularId,
                MusculoPrimarioId = MusculoPrimarioId
            };

            var musculosSecundariosIds = new HashSet<int>();

            if (ejercicioJson.MusculosSecundarios != null)
            {
                foreach (var musculoSec in ejercicioJson.MusculosSecundarios!
                        .Where(ms => !string.IsNullOrWhiteSpace(ms.Nombre)))
                {
                    var nombreMusculoSec = Normalizar(musculoSec.Nombre!);

                    if (musculosDict.TryGetValue(nombreMusculoSec, out var musculoSecId))
                    {

                        if (musculoSecId != MusculoPrimarioId &&
                            !musculosSecundariosIds.Contains(musculoSecId))
                        {

                            nuevoEjercicio.MusculosSecundarios.Add(
                                new Musculos { MusculoId = musculoSecId }
                            );

                            musculosSecundariosIds.Add(musculoSecId);
                        }
                    }
                }
            }            
            return nuevoEjercicio;
        }
        private Dictionary<string, int> ProcesarMusculosAsync(List<string> musculosJson, ImportResultDTO resultado, Dictionary<string, int> musculosDb)
        {
            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            // cargar existentes
            foreach (var kv in musculosDb)
                dict[kv.Key] = kv.Value;

            var nombresNuevos = musculosJson
                .Select(Normalizar)
                .Where(n => !dict.ContainsKey(n))
                .Distinct()
                .ToList();

            foreach (var nombre in nombresNuevos)
            {
                var nuevo = new Musculos
                {
                    Nombre = nombre
                };

                appDbContext.Musculos.Add(nuevo);

                resultado.MusculosCreados++;
                _logger.LogDebug($"Creado musculo: {nombre}");
            }

            return dict;
        }
        private Dictionary<string,int> ProcesarGruposMuscularesAsync(List<string> nombresGruposMusculares, ImportResultDTO resultado, Dictionary<string, int> gruposExistentes)
        {
            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            // cargar existentes
            foreach (var kv in gruposExistentes)
                dict[kv.Key] = kv.Value;

            var nombresNuevos = nombresGruposMusculares
                .Select(Normalizar)
                .Where(n => !dict.ContainsKey(n))
                .Distinct()
                .ToList();

            foreach (var nombre in nombresNuevos)
            {
                var nuevo = new GrupoMuscular
                {
                    Nombre = nombre
                };

                appDbContext.GrupoMusculares.Add(nuevo);

                resultado.GruposMuscularesCreados++;
            }

            return dict;
        }        
        private bool ValidarEjercicioImport(EjercicioDTO ejercicioImport, int indice, ImportResultDTO resultado)
        {
            if (string.IsNullOrWhiteSpace(ejercicioImport.Nombre))
            {
                resultado.Errores.Add(new ImportErrorDTO
                {
                    Indice = indice,
                    Mensaje = "El nombre del ejercicio está vacío"
                });
                return false;
            }

            if (string.IsNullOrWhiteSpace(ejercicioImport.GrupoMuscular.Nombre))
            {
                resultado.Errores.Add(new ImportErrorDTO
                {
                    Indice = indice,
                    NombreEjercicio = ejercicioImport.Nombre,
                    Mensaje = "No se especificó grupo muscular"
                });
                return false;
            }

            if (ejercicioImport.MusculoPrimario == null ||
                string.IsNullOrWhiteSpace(ejercicioImport.MusculoPrimario.Nombre))
            {
                resultado.Errores.Add(new ImportErrorDTO
                {
                    Indice = indice,
                    NombreEjercicio = ejercicioImport.Nombre,
                    Mensaje = "No se especificó músculo primario"
                });
                return false;
            }

            ejercicioImport.MusculosSecundarios ??= new List<MusculoDTO>();

            return true;
        }
        private string ObtenerMensajeErrorDb(DbUpdateException dbEx)
        {
            if (dbEx.InnerException is SqlException sqlEx)
            {
                switch (sqlEx.Number)
                {
                    case 2627: // Unique constraint
                    case 2601:
                        return "Violación de unicidad (registro duplicado)";

                    case 547: // Foreign key
                        return "Violación de clave foránea (relación inválida)";

                    default:
                        return $"SQL Server error ({sqlEx.Number}): {sqlEx.Message}";
                }
            }
            return dbEx.Message;
        }
        private void AgregarError(int indice, string? nombreEjercicio, string mensaje, ImportResultDTO resultado, string? stackTrace = null)
        {
            resultado.Errores.Add(new ImportErrorDTO
            {
                Indice = indice,
                NombreEjercicio = nombreEjercicio,
                Mensaje = mensaje,
                StackTrace = stackTrace
            });
        }
        private string Normalizar(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return string.Empty;

            // Normaliza espacios múltiples + unicode
            var limpio = Regex.Replace(nombre, @"\s+", " ");

            return limpio.Trim().ToLowerInvariant();
        }
    }
}
