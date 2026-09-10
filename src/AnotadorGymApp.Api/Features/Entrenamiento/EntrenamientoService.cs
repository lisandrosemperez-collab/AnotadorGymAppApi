using AnotadorGymApp.Api.Domain.Entities.Entrenamiento;
using AnotadorGymApp.Api.Features.Entrenamiento.DTOs;
using AnotadorGymAppApi.Domain.Entities.Ejercicio;
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
            var entidad = new AnotadorGymApp.Api.Domain.Entities.Entrenamiento.Entrenamiento
            {
                UsuarioId = usuarioId,
                Fecha = DateTime.Now,
                UltimaActualizacion = DateTime.Now,
                RutinaDiaId = dto.RutinaDiaId,
                Completado = false,
                Notas = dto.Notas ?? string.Empty
            };

            _db.Entrenamientos.Add(entidad);

            await _db.SaveChangesAsync(cancellationToken);

            return MapToDto(entidad);
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
            if (!dto.EntrenamientoId.HasValue
                && dto.EntrenamientoId != 0) return false;

            var entrenamiento = await _db.Entrenamientos
            .Include(e => e.Ejercicios)
                .ThenInclude(ee => ee.Series)
            .FirstOrDefaultAsync(
                e => e.EntrenamientoId == dto.EntrenamientoId &&
                        e.UsuarioId == usuarioId,
                cancellationToken);

            if (entrenamiento == null) return false;

            // Actualizar propiedades de root
            SincronizarRootEntrenamiento(entrenamiento, dto);

            // Obtener ejercicios del DTO
            var ejerciciosDto = dto.Ejercicios ?? new List<EjercicioEntrenadoDto>();

            // 1) Eliminar ejercicios que no están en el DTO
            var ejercicioIds = ejerciciosDto
                .Select(e => e.EjercicioId)
                .Distinct()
                .ToList();

            if (ejercicioIds.Any(id => id <= 0))
                return false;

            // Verificar que todos los ejercicios existan en la DB
            var ejerciciosExistentes = await _db.Ejercicios
                .Where(e => ejercicioIds.Contains(e.EjercicioId))
                .Select(e => e.EjercicioId)
                .ToListAsync(cancellationToken);

            if (ejerciciosExistentes.Count != ejercicioIds.Count)
                return false;

            // IDs de los EjercicioEntrenado que siguen presentes en el DTO
            var ejerciciosConId = ejerciciosDto
                .Where(e =>
                    e.EjercicioEntrenadoId.HasValue &&
                    e.EjercicioEntrenadoId.Value != 0)
                .Select(e => e.EjercicioEntrenadoId!.Value)
                .ToHashSet();

            // Eliminar ejercicios que ya no vienen en el DTO
            var ejerciciosAEliminar = entrenamiento.Ejercicios
                .Where(dbEe =>
                    !ejerciciosConId.Contains(dbEe.EjercicioEntrenadoId))
                .ToList();

            foreach (var rem in ejerciciosAEliminar)
            {
                entrenamiento.Ejercicios.Remove(rem);
            }

            // 2) Actualizar existentes y crear nuevos
            foreach (var ejercicioDto in ejerciciosDto)
            {
                if (ejercicioDto.EjercicioEntrenadoId.HasValue &&
            ejercicioDto.EjercicioEntrenadoId.Value != 0)
                {
                    var dbEjercicio = entrenamiento.Ejercicios
                        .FirstOrDefault(e =>
                            e.EjercicioEntrenadoId ==
                            ejercicioDto.EjercicioEntrenadoId.Value);

                    if (dbEjercicio is null)
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
                    entrenamiento.Ejercicios.Add(
                        CrearEjercicio(ejercicioDto));
                }
            }

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

            // Sincronizar series
            var dtoSeries = eeDto.Series ?? new List<SerieEntrenadaDto>();

            return SincronizarSeries(dbEe, dtoSeries);
        }

        private static bool SincronizarSeries(EjercicioEntrenado dbEe, List<SerieEntrenadaDto> seriesDto)
        {
            var dtoIds = seriesDto
                .Where(s => s.SerieEntrenadaId.HasValue)
                .Select(s => s.SerieEntrenadaId!.Value)
                .ToHashSet();

            // eliminar series que no vienen en DTO
            var seriesEliminar = dbEe.Series
                .Where(s => 
                    !dtoIds.Contains(s.SerieEntrenadaId))
                .ToList();

            foreach (var r in seriesEliminar) dbEe.Series.Remove(r);

            // actualizar/crear

            foreach (var serieDto in seriesDto)
            {
                if (serieDto.SerieEntrenadaId.HasValue &&
                    serieDto.SerieEntrenadaId.Value != 0)
                {
                    var serieDb = dbEe.Series
                        .FirstOrDefault(s => s.SerieEntrenadaId == serieDto.SerieEntrenadaId.Value);
                    
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
