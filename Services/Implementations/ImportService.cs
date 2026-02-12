using AnotadorGymAppApi.Context;
using AnotadorGymAppApi.DTOs.Ejercicio;
using AnotadorGymAppApi.DTOs.ImportResult;
using AnotadorGymAppApi.DTOs.Musculo;
using AnotadorGymAppApi.Models;
using AnotadorGymAppApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AnotadorGymAppApi.Services.Implementations
{
    public class ImportService : IImportService
    {
        private readonly AppDbContext appDbContext;
        private readonly ILogger<ImportService> _logger;
        public ImportService(AppDbContext appDbContext,ILogger<ImportService> logger)
        {
            this.appDbContext = appDbContext;
            _logger = logger;
        }

        public async Task<ImportResultDTO> ImportarEjerciciosDesdeArchivoAsync(IFormFile archivo)
        {
            _logger.LogInformation($"Iniciando importación desde archivo: {archivo.FileName}");
            var inicio = DateTime.UtcNow;
            try
            {
                using var stream = archivo.OpenReadStream();
                using var reader = new StreamReader(stream);
                var jsonContent = await reader.ReadToEndAsync();

                var ejerciciosImport = JsonSerializer.Deserialize<List<EjercicioDTO>>(jsonContent,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                if (ejerciciosImport == null || !ejerciciosImport.Any())
                {
                    throw new ArgumentException("El archivo JSON está vacío o es inválido");
                }

                var resultado = await ImportarEjerciciosDesdeJsonAsync(ejerciciosImport);
                resultado.Duracion = DateTime.UtcNow - inicio;

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar archivo JSON");

                // ✅ Crea un resultado de error con duración
                var resultado = new ImportResultDTO
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
            var inicio = DateTime.UtcNow;

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

                _logger.LogInformation($"Procesando {gruposMuscularesJson.Count} grupos musculares");


                // Obtener todos los grupos musculares existentes
                var gruposMuscularesDb = await appDbContext.GrupoMusculares
                    .AsNoTracking()
                    .ToDictionaryAsync(m => m.Nombre.ToLower(), m => m);
                
                // Procesar grupos musculares
                var gruposMuscularesDict = await ProcesarGruposMuscularesAsync(gruposMuscularesJson, resultado,gruposMuscularesDb);

                await appDbContext.SaveChangesAsync();

                _logger.LogInformation($"Procesando {musculosJson.Count} músculos");
                
                var musculosDb = await appDbContext.Musculos
                .AsNoTracking()
                .ToDictionaryAsync(m => m.Nombre.ToLower(), m => m);

                var musculosDict = await ProcesarMusculosAsync(musculosJson, resultado, musculosDb);

                await appDbContext.SaveChangesAsync();

                _logger.LogInformation($"Procesando {ejerciciosJson.Count} ejercicios");                

                await using var transaction = await appDbContext.Database.BeginTransactionAsync();

                try
                {
                    var ejerciciosDb = await appDbContext.Ejercicios
                    .Include(e => e.MusculosSecundarios)
                    .AsNoTracking()
                    .ToDictionaryAsync(e => e.Nombre.ToLower(), e => e);

                    await ProcesarEjerciciosAsync(
                        ejerciciosJson, gruposMuscularesDict, musculosDict, resultado, ejerciciosDb);
                    
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
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la importación de ejercicios");
                resultado.Errores.Add(new ImportErrorDTO
                {
                    Mensaje = $"Error general: {ex.Message}",
                    StackTrace = ex.StackTrace
                });
                resultado.Duracion = DateTime.UtcNow - inicio;
            }
            
            var duracion = DateTime.UtcNow - inicio;
            _logger.LogInformation("Importación completada en {Duracion}ms. " +
                "Creados: {Creados}, Actualizados: {Actualizados}, Errores: {Errores}",
                duracion.TotalMilliseconds,
                resultado.EjerciciosCreados,
                resultado.EjerciciosActualizados,
                resultado.Errores.Count);

            if (resultado.Duracion == TimeSpan.Zero)
            {
                resultado.Duracion = DateTime.UtcNow - inicio;
            }

            return resultado;

        }
        private async Task ProcesarEjerciciosAsync(List<EjercicioDTO> ejerciciosJson, Dictionary<string, GrupoMuscular> gruposMuscularesDict, Dictionary<string, Musculos> musculosDict, ImportResultDTO importResult, Dictionary<string, Ejercicio> ejerciciosDb)
        {
            int batchSize = 100;
            int ejerciciosCreadosEnLote = 0;
            int totalProcesados = 0;


            var ejerciciosValidados = new List<(Ejercicio Ejercicio, int IndiceOriginal)>();
            var nombresProcesados = new HashSet<string>();
            var indicesDuplicados = new List<int>();

            for (int i = 0; i < ejerciciosJson.Count; i++)
            {
                var ejercicioJson = ejerciciosJson[i];
                if (!string.IsNullOrWhiteSpace(ejercicioJson.Nombre))
                {
                    var nombreNormalizado = ejercicioJson.Nombre.Trim().ToLower();
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

                    var nombreNormalizado = ejercicioJson.Nombre!.Trim().ToLower();

                    // Buscar grupo muscular
                    if (!gruposMuscularesDict.TryGetValue(
                        ejercicioJson.GrupoMuscular.Nombre!.Trim().ToLower(),
                        out var grupoMuscular))
                    {
                        AgregarError(i, ejercicioJson.Nombre,
                            $"Grupo muscular '{ejercicioJson.GrupoMuscular}' no encontrado",
                            importResult);
                        continue;
                    }
                    // Buscar musculo primario
                    if (!musculosDict.TryGetValue(
                        ejercicioJson.MusculoPrimario!.Nombre!.Trim().ToLower(),
                        out var musculoPrimario))
                    {
                        AgregarError(i, ejercicioJson.Nombre,
                            $"Músculo primario '{ejercicioJson.MusculoPrimario.Nombre}' no encontrado",
                            importResult);
                        continue;
                    }
                    // Verificar si ya existe en BD (validación rápida con diccionario)
                    if (ejerciciosDb.ContainsKey(nombreNormalizado))
                    {
                        importResult.EjerciciosOmitidos++;
                        _logger.LogDebug($"Ejercicio {ejercicioJson.Nombre} ya existe, se omite");
                        continue;
                    }
                    #endregion

                    // Crear nuevo ejercicio
                    var nuevoEjercicio = await CrearNuevoEjercicioAsync(
                        ejercicioJson, grupoMuscular, musculoPrimario,
                        musculosDict);

                    await ValidarEjercicioAntesDeGuardarAsync(
                        nuevoEjercicio, gruposMuscularesDict, musculosDict,
                        ejerciciosDb, i, importResult);


                    ejerciciosValidados.Add((nuevoEjercicio, i));                                        
                    ejerciciosDb[nombreNormalizado] = nuevoEjercicio;

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
                        appDbContext.Ejercicios.Add(ejercicio);
                    }

                    // ✅ INTENTAR GUARDAR EL BATCH
                    await appDbContext.SaveChangesAsync();

                    ejerciciosCreadosEnLote += batch.Count;
                    importResult.EjerciciosCreados += batch.Count;

                    _logger.LogInformation(
                        $"Batch {(batchIndex / batchSize) + 1} guardado exitosamente: {batch.Count} ejercicios creados");
                }
                catch (DbUpdateException dbEx)
                {
                    // ✅ ¡ERROR EN EL BATCH! Ahora guardamos uno por uno para identificar el problema
                    _logger.LogWarning(dbEx,$"Error al guardar batch {(batchIndex / batchSize) + 1}. Guardando ejercicios individualmente...");

                    await GuardarEjerciciosIndividualmenteAsync(
                        batch, importResult, gruposMuscularesDict, musculosDict, ejerciciosDb);
                }
            }            
        }
        private async Task GuardarEjerciciosIndividualmenteAsync(
                        List<(Ejercicio Ejercicio, int IndiceOriginal)> batch,
                        ImportResultDTO resultado,
                        Dictionary<string, GrupoMuscular> gruposMuscularesDict,
                        Dictionary<string, Musculos> musculosDict,
                        Dictionary<string, Ejercicio> todosEjercicios)
        {
            foreach (var (ejercicio, indiceOriginal) in batch)
            {
                await using var individualTransaction = await appDbContext.Database.BeginTransactionAsync();
                try
                {
                    // Limpiar el contexto para empezar fresco
                    appDbContext.ChangeTracker.Clear();

                    // Volver a agregar solo este ejercicio
                    appDbContext.Ejercicios.Add(ejercicio);

                    // ✅ GUARDAR SOLO ESTE EJERCICIO
                    await appDbContext.SaveChangesAsync();
                    await individualTransaction.CommitAsync();

                    resultado.EjerciciosCreados++;

                    _logger.LogDebug($"Ejercicio creado individualmente: {ejercicio.Nombre}");
                }
                catch (DbUpdateException dbEx)
                {
                    await individualTransaction.RollbackAsync();
                    // ✅ ¡AHORA SABEMOS EXACTAMENTE QUÉ EJERCICIO FALLÓ!
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
        private async Task<Ejercicio> CrearNuevoEjercicioAsync(EjercicioDTO ejercicioJson, GrupoMuscular grupoMuscular, Musculos musculoPrimario, Dictionary<string, Musculos> musculosDict)
        {
            var nuevoEjercicio = new Ejercicio
            {
                Nombre = ejercicioJson.Nombre,
                Descripcion = ejercicioJson.Descripcion ??
                         $"Ejercicio para {musculoPrimario.Nombre}",
                GrupoMuscularId = grupoMuscular.GrupoMuscularId,
                MusculoPrimarioId = musculoPrimario.MusculoId,                
            };

            var musculosSecundariosIds = new HashSet<int>();
            if (ejercicioJson.MusculosSecundarios != null)
            {
                foreach (var musculoSec in ejercicioJson.MusculosSecundarios!
                        .Where(ms => !string.IsNullOrWhiteSpace(ms.Nombre)))
                {
                    var nombreMusculoSec = musculoSec.Nombre!.Trim().ToLower();
                    if (musculosDict.TryGetValue(nombreMusculoSec, out var musculoSecObj))
                    {

                        if (musculoSecObj.MusculoId != musculoPrimario.MusculoId &&
                            !musculosSecundariosIds.Contains(musculoSecObj.MusculoId))
                        {
                            nuevoEjercicio.MusculosSecundarios.Add(musculoSecObj);
                            musculosSecundariosIds.Add(musculoSecObj.MusculoId);
                        }
                    }
                }
            }            
            return nuevoEjercicio;
        }
        private async Task<Dictionary<string, Musculos>> ProcesarMusculosAsync(List<string> musculosJson, ImportResultDTO resultado, Dictionary<string, Musculos> musculosDb)
        {
            var MusculosDict = new Dictionary<string, Musculos>(StringComparer.OrdinalIgnoreCase);

            foreach (var nombre in musculosJson)
            {
                var musculoJsonLower = nombre.Trim().ToLower();
                if (musculosDb.TryGetValue(musculoJsonLower, out var musculoExistente))
                {
                    MusculosDict[musculoJsonLower] = musculoExistente;
                }
            }

            var nombresNuevos = musculosJson
                .Where(nombre => !MusculosDict.ContainsKey(nombre.ToLower()))
                .ToList();

            try
            {
                foreach (var nombreNuevo in nombresNuevos)
                {
                    var nuevoMusculo = new Musculos
                    {
                        Nombre = nombreNuevo
                    };
                    appDbContext.Musculos.Add(nuevoMusculo);
                    MusculosDict[nombreNuevo.ToLower()] = nuevoMusculo;
                    resultado.MusculosCreados++;
                    _logger.LogDebug($"Creado musculo: {nuevoMusculo.Nombre}");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error al crear nuevos músculos");
                resultado.Errores.Add(new ImportErrorDTO
                {
                    Mensaje = "Error al crear nuevos músculos: " + ex.Message
                });
            }   

            _logger.LogInformation($"Músculos procesados. Existentes: {musculosDb.Count}, Nuevos: {nombresNuevos.Count}");
            
            return MusculosDict;
        }
        private async Task<Dictionary<string,GrupoMuscular>> ProcesarGruposMuscularesAsync(List<string> nombresGruposMusculares, ImportResultDTO resultado, Dictionary<string, GrupoMuscular> todosGruposMusculares)
        {            
            var musculosDict = new Dictionary<string, GrupoMuscular>(StringComparer.OrdinalIgnoreCase);

            foreach (var nombre in nombresGruposMusculares)
            {
                var nombreLower = nombre.Trim().ToLower();
                if (todosGruposMusculares.TryGetValue(nombreLower, out var grupoExistente))
                {
                    musculosDict[nombreLower] = grupoExistente;
                }
            }

            var nombresNuevos = nombresGruposMusculares
                .Where(nombre => !musculosDict.ContainsKey(nombre.ToLower()))
                .Distinct()
                .ToList();

            try
            {
                foreach (var nombreNuevo in nombresNuevos)
                {
                    
                    var nuevoGrupo = new GrupoMuscular
                    {
                        Nombre = nombreNuevo
                    };                    
                    appDbContext.GrupoMusculares.Add(nuevoGrupo);
                    musculosDict[nombreNuevo.ToLower()] = nuevoGrupo;
                    resultado.GruposMuscularesCreados++;
                    _logger.LogDebug($"Creado grupo muscular: {nuevoGrupo.Nombre}");
                }               

            }catch (Exception ex)
            {                
                _logger.LogError(ex, "Error al crear nuevos grupos musculares");
                resultado.Errores.Add(new ImportErrorDTO
                {
                    Mensaje = "Error al crear nuevos grupos musculares: " + ex.Message,
                    StackTrace= ex.StackTrace,                    

                });
            }

            _logger.LogInformation($"Grupos musculares procesados. Existentes: {todosGruposMusculares.Count}, Nuevos: {nombresNuevos.Count}");
                        
            return musculosDict;
        }
        private async Task ActualizarEjercicioExistenteAsync(Ejercicio ejercicioExistente, EjercicioDTO ejercicioImport, GrupoMuscular grupoMuscular, Musculos musculoPrimario, Dictionary<string, Musculos> musculosDict)
        {
            ejercicioExistente.Descripcion = ejercicioImport.Descripcion?.Trim()?? ejercicioExistente.Descripcion;
            
            ejercicioExistente.GrupoMuscularId = grupoMuscular.GrupoMuscularId;
            ejercicioExistente.GrupoMuscular = grupoMuscular;

            ejercicioExistente.MusculoPrimarioId = musculoPrimario.MusculoId;
            ejercicioExistente.MusculoPrimario = musculoPrimario;

            var nuevosSecundarios = new List<Musculos>();
            var nuevosSecundariosIds = new HashSet<int>();

            if (ejercicioImport.MusculosSecundarios != null)
            {

                foreach (var musculoSec in ejercicioImport.MusculosSecundarios!
                        .Where(ms => !string.IsNullOrWhiteSpace(ms.Nombre)))
                {
                    var nombreMusculoSec = musculoSec.Nombre!.Trim().ToLower();
                    if (musculosDict.TryGetValue(nombreMusculoSec, out var musculoSecObj))
                    {
                        // Evitar que sea el mismo que el primario y duplicados
                        if (musculoSecObj.MusculoId != musculoPrimario.MusculoId &&
                            !nuevosSecundariosIds.Contains(musculoSecObj.MusculoId))
                        {
                            nuevosSecundarios.Add(musculoSecObj);
                            nuevosSecundariosIds.Add(musculoSecObj.MusculoId);
                        }
                    }
                }
            }

            if (!appDbContext.Entry(ejercicioExistente).Collection(e => e.MusculosSecundarios).IsLoaded)
            {
                await appDbContext.Entry(ejercicioExistente)
                    .Collection(e => e.MusculosSecundarios)
                    .LoadAsync();
            }

            ejercicioExistente.MusculosSecundarios.Clear();
            foreach (var secundario in nuevosSecundarios)
            {
                ejercicioExistente.MusculosSecundarios.Add(secundario);
            }
        }
        private string ObtenerMensajeErrorDb(DbUpdateException dbEx)
        {
            if (dbEx.InnerException is PostgresException pgEx)
            {
                switch (pgEx.SqlState)
                {
                    case "23505": // Violación de unicidad
                        return $"Violación de unicidad en {pgEx.ConstraintName}: {pgEx.MessageText}";
                    case "23503": // Violación de clave foránea
                        return $"Clave foránea no encontrada: {pgEx.ConstraintName}";
                    case "23514": // Violación de check constraint
                        return $"Restricción check violada: {pgEx.ConstraintName}";
                    default:
                        return $"Error PostgreSQL ({pgEx.SqlState}): {pgEx.MessageText}";
                }
            }

            return dbEx.Message;
        }
        private void AgregarError(int indice,string? nombreEjercicio,string mensaje,ImportResultDTO resultado,string? stackTrace = null)
        {
            resultado.Errores.Add(new ImportErrorDTO
            {
                Indice = indice,
                NombreEjercicio = nombreEjercicio,
                Mensaje = mensaje,
                StackTrace = stackTrace
            });
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
        private async Task ValidarEjercicioAntesDeGuardarAsync(
                        Ejercicio ejercicio,
                        Dictionary<string, GrupoMuscular> gruposMuscularesDict,
                        Dictionary<string, Musculos> musculosDict,
                        Dictionary<string, Ejercicio> ejerciciosDb,
                        int indice,
                        ImportResultDTO resultado)
        {
            try
            {
                // 1. Validar propiedades requeridas
                if (string.IsNullOrWhiteSpace(ejercicio.Nombre))
                    throw new InvalidOperationException("El nombre del ejercicio está vacío");

                if (ejercicio.GrupoMuscularId <= 0)
                    throw new InvalidOperationException("Grupo muscular no válido (ID menor o igual a 0)");

                if (ejercicio.MusculoPrimarioId <= 0)
                    throw new InvalidOperationException("Músculo primario no válido (ID menor o igual a 0)");

                // 2. Validar que no exista duplicado en la base de datos (consulta directa para mayor seguridad)
                var existeEnBD = await appDbContext.Ejercicios
                    .AnyAsync(e => e.Nombre.ToLower() == ejercicio.Nombre.ToLower());

                if (existeEnBD)
                    throw new InvalidOperationException($"El ejercicio '{ejercicio.Nombre}' ya existe en la base de datos");

                // 3. Validar que el grupo muscular exista en la base de datos
                var grupoExiste = await appDbContext.GrupoMusculares
                    .AnyAsync(g => g.GrupoMuscularId == ejercicio.GrupoMuscularId);

                if (!grupoExiste)
                    throw new InvalidOperationException($"El grupo muscular con ID {ejercicio.GrupoMuscularId} no existe");

                // 4. Validar que el músculo primario exista en la base de datos
                var musculoPrimarioExiste = await appDbContext.Musculos
                    .AnyAsync(m => m.MusculoId == ejercicio.MusculoPrimarioId);

                if (!musculoPrimarioExiste)
                    throw new InvalidOperationException($"El músculo primario con ID {ejercicio.MusculoPrimarioId} no existe");

                // 5. Validar músculos secundarios
                foreach (var musculoSec in ejercicio.MusculosSecundarios)
                {
                    var musculoSecExiste = await appDbContext.Musculos
                        .AnyAsync(m => m.MusculoId == musculoSec.MusculoId);

                    if (!musculoSecExiste)
                        throw new InvalidOperationException($"Músculo secundario con ID {musculoSec.MusculoId} no existe");

                    // Validar que no sea el mismo que el primario
                    if (musculoSec.MusculoId == ejercicio.MusculoPrimarioId)
                        throw new InvalidOperationException($"El músculo secundario '{musculoSec.Nombre}' no puede ser el mismo que el primario");
                }

                // 6. Validar duplicados en secundarios
                var idsSecundarios = ejercicio.MusculosSecundarios.Select(m => m.MusculoId).ToList();
                if (idsSecundarios.Distinct().Count() != idsSecundarios.Count)
                    throw new InvalidOperationException("Hay músculos secundarios duplicados");

                // 7. Validar relaciones de navegación (si están cargadas)
                if (ejercicio.GrupoMuscular != null && ejercicio.GrupoMuscular.GrupoMuscularId != ejercicio.GrupoMuscularId)
                    throw new InvalidOperationException("Inconsistencia entre GrupoMuscularId y objeto GrupoMuscular");

                if (ejercicio.MusculoPrimario != null && ejercicio.MusculoPrimario.MusculoId != ejercicio.MusculoPrimarioId)
                    throw new InvalidOperationException("Inconsistencia entre MusculoPrimarioId y objeto MusculoPrimario");
            }
            catch (Exception ex)
            {
                // Registrar el error específico
                resultado.Errores.Add(new ImportErrorDTO
                {
                    Indice = indice,
                    NombreEjercicio = ejercicio.Nombre,
                    Mensaje = $"Validación falló: {ex.Message}",
                    StackTrace = ex.StackTrace
                });
                throw; // Re-lanzar para que el caller sepa
            }
        }
    }
}
