using AnotadorGymApp.Api.Domain.Entities.Entrenamiento;
using AnotadorGymApp.Api.Features.Entrenamiento.DTOs;
using AnotadorGymAppApi.Domain.Entities.Ejercicio;
using AnotadorGymAppApi.Domain.Entities.Rutina;
using AnotadorGymAppApi.Features.Ejercicios.DTOs;
using AnotadorGymAppApi.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AnotadorGymApp.Api.Features.Entrenamiento
{
    public class EntrenamientoService : IEntrenamientoService
    {
        private readonly AppDbContext _db;
        public EntrenamientoService(AppDbContext db)
        {
            _db = db;
        }
        public async Task<EntrenamientoDto?> ObtenerPorIdAsync(int entrenamientoId, int usuarioId, CancellationToken cancellationToken)
        {
            var ent = await _db.Entrenamientos
                .Include(e => e.Ejercicios)
                    .ThenInclude(ee => ee.Series)
                .Include(e => e.Ejercicios)
                    .ThenInclude(ee => ee.Ejercicio)
                .Include(e => e.RutinaDia)
                    .ThenInclude(rd => rd.RutinaSemana)
                        .ThenInclude(rs => rs.Rutina)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EntrenamientoId == entrenamientoId && e.UsuarioId == usuarioId, cancellationToken);

            if (ent == null) return null;

            return MapToDto(ent);
        }
        public async Task<IEnumerable<EntrenamientoDto>> ObtenerPorUsuarioAsync(int usuarioId, CancellationToken cancellationToken)
        {
            var list = await _db.Entrenamientos
                .Include(e => e.Ejercicios)
                    .ThenInclude(ee => ee.Series)
                .Include(e => e.Ejercicios)
                    .ThenInclude(ee => ee.Ejercicio)
                .Include(e => e.RutinaDia)
                    .ThenInclude(rd => rd.RutinaSemana)
                        .ThenInclude(rs => rs.Rutina)
                .AsNoTracking()
                .Where(e => e.UsuarioId == usuarioId)
                .OrderByDescending(e => e.Fecha)
                .ToListAsync(cancellationToken);

            return list.Select(MapToDto).ToList();
        }
        public async Task<EntrenamientoDto?> ObtenerEntrenamientoDelDiaAsync(int usuarioId, CancellationToken cancellationToken)
        {
            var today = DateTime.Today;

            var entrenamiento = await _db.Entrenamientos
                .Include(e => e.Ejercicios)
                    .ThenInclude(ee => ee.Series)
                .Include(e => e.Ejercicios)
                    .ThenInclude(ee => ee.Ejercicio)
                .Include(e => e.RutinaDia)
                    .ThenInclude(rd => rd.RutinaSemana)
                        .ThenInclude(rs => rs.Rutina)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.UsuarioId == usuarioId && e.Fecha.Date == today && e.Completado == false);

            return entrenamiento is not null ? MapToDto(entrenamiento) : null;
        }        
        public async Task<EntrenamientoDto> CrearAsync(EntrenamientoDto dto, int usuarioId, CancellationToken cancellationToken)
        {
            // 1. Buscar rutina del día
            var rutinaDia = await _db.RutinaDias
                .Include(rd => rd.Ejercicios)
                    .ThenInclude(re => re.Series)
                .FirstOrDefaultAsync(
                    rd => rd.RutinaDiaId == dto.RutinaDiaId,
                    cancellationToken);

            if (rutinaDia is null)
            {
                throw new KeyNotFoundException(
                    $"No se encontró el RutinaDia {dto.RutinaDiaId}.");
            }

            // 2. Crear entrenamiento
            var entidad = new AnotadorGymApp.Api.Domain.Entities.Entrenamiento.Entrenamiento
            {
                UsuarioId = usuarioId,
                Fecha = DateTime.Now,
                UltimaActualizacion = DateTime.Now,
                RutinaDiaId = dto.RutinaDiaId,
                Completado = false,
                Notas = dto.Notas ?? string.Empty
            };

            // 3. Guardar SOLO el entrenamiento
            _db.Entrenamientos.Add(entidad);

            await _db.SaveChangesAsync(cancellationToken);

            // 4. Crear ejercicios entrenados EN MEMORIA
            foreach (var rutinaEjercicio in rutinaDia.Ejercicios
                .OrderBy(re => re.NumeroEjercicio))
            {
                var ejercicioEntrenado = new EjercicioEntrenado
                {
                    EjercicioId = rutinaEjercicio.EjercicioId,
                    Orden = rutinaEjercicio.NumeroEjercicio,
                    Notas = string.Empty,
                    Completado = false
                };

                // 5. Crear series entrenadas EN MEMORIA
                foreach (var rutinaSerie in rutinaEjercicio.Series
                    .OrderBy(rs => rs.NumeroSerie))
                {
                    var serieEntrenada = new SerieEntrenada
                    {
                        NumeroSerie = rutinaSerie.NumeroSerie,

                        // Estado inicial de la serie real
                        Peso = 0,
                        Repeticiones = 0,
                        Completada = false,
                        FuePR = false,
                        RPE = null,
                        DescansoSegundos = 0
                    };

                    ejercicioEntrenado.Series.Add(serieEntrenada);
                }

                entidad.Ejercicios.Add(ejercicioEntrenado);
            }

            // 6. Convertir a DTO
            var resultado = MapToDto(entidad);

            // 7. Agregar información de referencia
            AgregarReferencias(resultado, rutinaDia);

            return resultado;
        }        

        public async Task<bool> BorrarAsync(int usuarioId, int entrenamientoId, CancellationToken cancellationToken)
        {
            var entrenamiento = await _db.Entrenamientos
                .FirstOrDefaultAsync(e => e.EntrenamientoId == entrenamientoId && e.UsuarioId == usuarioId, cancellationToken);

            if (entrenamiento == null) return false;

            _db.Entrenamientos.Remove(entrenamiento);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
        public async Task<bool> SincronizarEntrenamiento(int usuarioId, EntrenamientoDto dto, CancellationToken cancellationToken)
        {
            // Validar ID del entrenamiento
            if (!dto.EntrenamientoId.HasValue ||
                dto.EntrenamientoId.Value <= 0)
            {
                return false;
            }

            var entrenamiento = await _db.Entrenamientos
            .Include(e => e.Ejercicios)
                .ThenInclude(ee => ee.Series)
            .FirstOrDefaultAsync(
                e => e.EntrenamientoId == dto.EntrenamientoId &&
                        e.UsuarioId == usuarioId,
                cancellationToken);

            if (entrenamiento == null) return false;

            // ---------------------------------------------------------
            // 1. Sincronizar datos generales del entrenamiento
            // ---------------------------------------------------------
            
            SincronizarRootEntrenamiento(entrenamiento, dto);

            // ---------------------------------------------------------
            // 2. Filtrar ejercicios y series no ejecutados
            // ---------------------------------------------------------

            var ejerciciosDto = (dto.Ejercicios ?? new List<EjercicioEntrenadoDto>())
                .Where(e =>
                    e.Series != null &&
                    e.Series.Any(s => s.Repeticiones > 0))
                .ToList();           

            // ---------------------------------------------------------
            // 3. Validar los ejercicios que realmente vamos a guardar
            // ---------------------------------------------------------
            var ejercicioIds = ejerciciosDto
                .Select(e => e.EjercicioId)
                .Distinct()
                .ToList();

            if (ejercicioIds.Any(id => id <= 0))
                return false;

            var ejerciciosExistentes = await _db.Ejercicios
                .Where(e => ejercicioIds.Contains(e.EjercicioId))
                .Select(e => e.EjercicioId)
                .ToListAsync(cancellationToken);

            if (ejerciciosExistentes.Count != ejercicioIds.Count)
                return false;

            // ---------------------------------------------------------
            // 4. Obtener IDs de los ejercicios que siguen existiendo
            // ---------------------------------------------------------
            var ejerciciosConId = ejerciciosDto
                .Where(e =>
                    e.EjercicioEntrenadoId.HasValue &&
                    e.EjercicioEntrenadoId.Value != 0)
                .Select(e => e.EjercicioEntrenadoId!.Value)
                .ToHashSet();

            // ---------------------------------------------------------
            // 5. Eliminar ejercicios que ya no tienen series ejecutadas
            // ---------------------------------------------------------
            var ejerciciosAEliminar = entrenamiento.Ejercicios
                .Where(dbEe =>
                    !ejerciciosConId.Contains(dbEe.EjercicioEntrenadoId))
                .ToList();

            foreach (var ejercicio in ejerciciosAEliminar)
            {
                entrenamiento.Ejercicios.Remove(ejercicio);
            }

            // ---------------------------------------------------------
            // 6. Actualizar existentes / crear nuevos
            // ---------------------------------------------------------
            foreach (var ejercicioDto in ejerciciosDto)
            {
                if (ejercicioDto.EjercicioEntrenadoId.HasValue &&
                    ejercicioDto.EjercicioEntrenadoId.Value != 0)
                {
                    // ---------------------------------------------
                    // Ejercicio existente
                    // ---------------------------------------------

                    var dbEjercicio = entrenamiento.Ejercicios
                        .FirstOrDefault(e =>
                            e.EjercicioEntrenadoId ==
                            ejercicioDto.EjercicioEntrenadoId.Value);

                    if (dbEjercicio == null)
                        return false;

                    if (!SincronizarEjercicioEntrenado(
                            dbEjercicio,
                            ejercicioDto))
                    {
                        return false;
                    }
                }
                else
                {
                    // ---------------------------------------------
                    // Ejercicio nuevo
                    // ---------------------------------------------

                    entrenamiento.Ejercicios.Add(
                        CrearEjercicio(ejercicioDto));
                }
            }

            // ---------------------------------------------------------
            // 7. Guardar cambios
            // ---------------------------------------------------------

            await _db.SaveChangesAsync(cancellationToken);

            return true;
        }


        // Helpers
        private static EntrenamientoDto MapToDto(Domain.Entities.Entrenamiento.Entrenamiento ent)
        {
            return new EntrenamientoDto
            {
                EntrenamientoId = ent.EntrenamientoId,
                Fecha = ent.Fecha,
                UltimaActualizacion = ent.UltimaActualizacion,
                RutinaDiaId = ent.RutinaDiaId,
                RutinaId = ent.RutinaDia?.RutinaSemana?.Rutina?.RutinaId ?? 0,
                DuracionSegundos = ent.DuracionSegundos,
                Notas = ent.Notas,
                Completado = ent.Completado,
                Ejercicios = ent.Ejercicios?.Select(ee => new EjercicioEntrenadoDto
                {
                    EjercicioEntrenadoId = ee.EjercicioEntrenadoId,
                    EjercicioId = ee.EjercicioId,
                    Ejercicio = ee.Ejercicio == null
                    ? null
                    : new EjercicioSimpleDTO
                    {
                        EjercicioId = ee.Ejercicio.EjercicioId,
                        Nombre = ee.Ejercicio.Nombre,
                        Descripcion = ee.Ejercicio.Descripcion,
                        UrlVideo = ee.Ejercicio.UrlVideo
                    },
                    Orden = ee.Orden,
                    Notas = ee.Notas,
                    Completado = ee.Completado,
                    Series = ee.Series?.Select(s => new SerieEntrenadaDto
                    {
                        SerieEntrenadaId = s.SerieEntrenadaId,
                        NumeroSerie = s.NumeroSerie,
                        Peso = s.Peso,
                        Repeticiones = s.Repeticiones,
                        Completada = s.Completada,
                        FuePR = s.FuePR,
                        RPE = s.RPE,
                        DescansoSegundos = s.DescansoSegundos
                    }).OrderBy(s => s.NumeroSerie).ToList()
                }).OrderBy(ee => ee.Orden).ToList()
            };
        }

        private static void AgregarReferencias(EntrenamientoDto dto, RutinaDia rutinaDia)
        {
            if (dto.Ejercicios == null)
                return;

            foreach (var ejercicioDto in dto.Ejercicios)
            {
                var rutinaEjercicio = rutinaDia.Ejercicios
                    .FirstOrDefault(re =>
                        re.EjercicioId == ejercicioDto.EjercicioId);

                if (rutinaEjercicio == null || ejercicioDto.Series == null)
                    continue;

                foreach (var serieDto in ejercicioDto.Series)
                {
                    var rutinaSerie = rutinaEjercicio.Series
                        .FirstOrDefault(rs =>
                            rs.NumeroSerie == serieDto.NumeroSerie);

                    if (rutinaSerie == null)
                        continue;

                    serieDto.SerieReferencia = new SerieReferenciaDto
                    {
                        Porcentaje1RM = rutinaSerie.Porcentaje1RM,
                        Repeticiones = rutinaSerie.Repeticiones,
                        DescansoSegundos = rutinaSerie.Descanso.HasValue
                            ? (int)rutinaSerie.Descanso.Value.TotalSeconds
                            : null
                    };
                }
            }
        }

        private static void SincronizarRootEntrenamiento(Domain.Entities.Entrenamiento.Entrenamiento dbEnt, EntrenamientoDto dto)
        {
            dbEnt.Fecha = dto.Fecha;
            dbEnt.DuracionSegundos = dto.DuracionSegundos;
            dbEnt.Notas = dto.Notas;
            dbEnt.Completado = dto.Completado;            
            dbEnt.UltimaActualizacion = DateTime.Now;
        }

        private static bool SincronizarEjercicioEntrenado(EjercicioEntrenado dbEe, EjercicioEntrenadoDto eeDto)
        {

            dbEe.EjercicioId = eeDto.EjercicioId;
            dbEe.Orden = eeDto.Orden;
            dbEe.Notas = eeDto.Notas;
            dbEe.Completado = eeDto.Completado;

            var dtoSeries = eeDto.Series ??
                new List<SerieEntrenadaDto>();

            return SincronizarSeries(dbEe, dtoSeries);
        }

        private static bool SincronizarSeries(EjercicioEntrenado dbEe, List<SerieEntrenadaDto> seriesDto)
        {

            var seriesValidas = seriesDto
               .Where(s => s.Repeticiones > 0)
               .ToList();

            var dtoIds = seriesValidas
                .Where(s =>
                    s.SerieEntrenadaId.HasValue &&
                    s.SerieEntrenadaId.Value != 0)
                .Select(s => s.SerieEntrenadaId!.Value)
                .ToHashSet();

            // Eliminar series que ya no están en el DTO válido            

            var seriesEliminar = dbEe.Series
                .Where(s => !dtoIds.Contains(s.SerieEntrenadaId))
                .ToList();

            foreach (var serie in seriesEliminar) dbEe.Series.Remove(serie);

            // Actualizar / crear solamente series válidas
            foreach (var serieDto in seriesValidas)
            {
                if (serieDto.SerieEntrenadaId.HasValue &&
                    serieDto.SerieEntrenadaId.Value != 0)
                {
                    var serieDb = dbEe.Series
                        .FirstOrDefault(s =>
                        s.SerieEntrenadaId == serieDto.SerieEntrenadaId.Value);
                    
                    if (serieDb == null) return false;
                    
                    serieDb.NumeroSerie = serieDto.NumeroSerie;
                    serieDb.Peso = serieDto.Peso;
                    serieDb.Repeticiones = serieDto.Repeticiones;
                    serieDb.Completada = serieDto.Completada;
                    serieDb.FuePR = serieDto.FuePR;
                    serieDb.RPE = serieDto.RPE;
                    serieDb.DescansoSegundos = serieDto.DescansoSegundos;                    
                }
                else
                {
                    dbEe.Series.Add(CrearSerie(serieDto));
                }
            }
            
            return true;
        }

        private static EjercicioEntrenado CrearEjercicio(EjercicioEntrenadoDto dto)
        {
            var ee = new EjercicioEntrenado
            {
                EjercicioId = dto.EjercicioId,
                Orden = dto.Orden,
                Notas = dto.Notas,
                Completado = false
            };
            if (dto.Series != null)
            {
                foreach (var sDto in dto.Series)
                {
                    ee.Series.Add(CrearSerie(sDto));
                }
            }
            return ee;
        }
        
        private static SerieEntrenada CrearSerie(SerieEntrenadaDto dto)
        {
            return new SerieEntrenada
            {
                NumeroSerie = dto.NumeroSerie,
                Peso = dto.Peso,
                Repeticiones = dto.Repeticiones,
                Completada = dto.Completada,
                FuePR = dto.FuePR,
                RPE = dto.RPE,
                DescansoSegundos = dto.DescansoSegundos
            };
        }
    }
}
