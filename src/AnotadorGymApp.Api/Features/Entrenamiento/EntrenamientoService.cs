using AnotadorGymApp.Api.Domain.Entities.Entrenamiento;
using AnotadorGymApp.Api.Features.Entrenamiento.DTOs;
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
                .AsNoTracking()
                .Where(e => e.UsuarioId == usuarioId)
                .OrderByDescending(e => e.Fecha)
                .ToListAsync(cancellationToken);

            return list.Select(MapToDto).ToList();
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

        public async Task<EntrenamientoDto> CrearAsync(EntrenamientoDto dto, int usuarioId, CancellationToken cancellationToken)
        {
            var entidad = new Domain.Entities.Entrenamiento.Entrenamiento
            {
                UsuarioId = usuarioId,
                Fecha = dto.Fecha,
                DuracionSegundos = dto.DuracionSegundos,
                Notas = dto.Notas
            };

            if (dto.Ejercicios != null)
            {
                foreach (var eeDto in dto.Ejercicios)
                {
                    var ee = new EjercicioEntrenado
                    {
                        EjercicioId = eeDto.EjercicioId,
                        Orden = eeDto.Orden,
                        Notas = eeDto.Notas
                    };

                    if (eeDto.Series != null)
                    {
                        foreach (var sDto in eeDto.Series)
                        {
                            ee.Series.Add(CrearSerie(sDto));
                        }
                    }

                    entidad.Ejercicios.Add(ee);
                }
            }

            _db.Entrenamientos.Add(entidad);
            await _db.SaveChangesAsync(cancellationToken);

            return MapToDto(entidad);
        }


        public async Task<bool> SincronizarEntrenamiento(int usuarioId, EntrenamientoDto dto, CancellationToken cancellationToken)
        {
            if (dto.EntrenamientoId == null) return false;

            var entrenamiento = await _db.Entrenamientos
                .Include(e => e.Ejercicios)
                    .ThenInclude(ee => ee.Series)
                .FirstOrDefaultAsync(e => e.EntrenamientoId == dto.EntrenamientoId && e.UsuarioId == usuarioId, cancellationToken);

            if (entrenamiento == null) return false;

            // Actualizar propiedades de root
            SincronizarRootEntrenamiento(entrenamiento, dto);

            // Sincronizar EjerciciosEntrenados
            var ejerciciosDto = dto.Ejercicios ?? new List<EjercicioEntrenadoDto>();

            // 1) Eliminar ejercicios que no están en el DTO
            var dtoEjIds = ejerciciosDto.Where(x => x.EjercicioEntrenadoId.HasValue).Select(x => x.EjercicioEntrenadoId!.Value).ToHashSet();
            var toRemove = entrenamiento.Ejercicios.Where(dbEe => !dtoEjIds.Contains(dbEe.EjercicioEntrenadoId)).ToList();
            
            foreach (var rem in toRemove)
            {
                entrenamiento.Ejercicios.Remove(rem);
            }

            // 2) Actualizar existentes y crear nuevos
            foreach (var ejercicioEntrenadoDto in ejerciciosDto)
            {
                if (ejercicioEntrenadoDto.EjercicioEntrenadoId.HasValue)
                {                    
                    var dbEjercicio = entrenamiento.Ejercicios
                        .FirstOrDefault(e => e.EjercicioEntrenadoId == ejercicioEntrenadoDto.EjercicioEntrenadoId.Value);

                    if (dbEjercicio is null)
                        return false;

                    SincronizarEjercicioEntrenado(dbEjercicio, ejercicioEntrenadoDto);
                }
                else
                {
                    if (ejercicioEntrenadoDto.EjercicioId == 0)
                        return false;

                    entrenamiento.Ejercicios.Add(CrearEjercicio(ejercicioEntrenadoDto));
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
                DuracionSegundos = ent.DuracionSegundos,
                Notas = ent.Notas,
                Ejercicios = ent.Ejercicios?.Select(ee => new EjercicioEntrenadoDto
                {
                    EjercicioEntrenadoId = ee.EjercicioEntrenadoId,
                    EjercicioId = ee.EjercicioId,
                    Orden = ee.Orden,
                    Notas = ee.Notas,
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
            dbEnt.Estado = dto.Finalizado ? EstadoEntrenamiento.Completado : EstadoEntrenamiento.EnCurso;
        }

        private static void SincronizarEjercicioEntrenado(EjercicioEntrenado dbEe, EjercicioEntrenadoDto eeDto)
        {            
            dbEe.EjercicioId = eeDto.EjercicioId;
            dbEe.Orden = eeDto.Orden;
            dbEe.Notas = eeDto.Notas;
            
            // Sincronizar series
            var dtoSeries = eeDto.Series ?? new List<SerieEntrenadaDto>();
            SincronizarSeries(dbEe, dtoSeries);
        }

        private static void SincronizarSeries(EjercicioEntrenado dbEe, List<SerieEntrenadaDto> seriesDto)
        {
            var dtoIds = seriesDto.Where(s => s.SerieEntrenadaId.HasValue).Select(s => s.SerieEntrenadaId!.Value).ToHashSet();

            // eliminar series que no vienen en DTO
            var toRemove = dbEe.Series.Where(s => !dtoIds.Contains(s.SerieEntrenadaId)).ToList();
            foreach (var r in toRemove) dbEe.Series.Remove(r);

            // actualizar/crear

            foreach (var sDto in seriesDto)
            {
                if (sDto.SerieEntrenadaId.HasValue && sDto.SerieEntrenadaId.Value != 0)
                {
                    var dbS = dbEe.Series.FirstOrDefault(s => s.SerieEntrenadaId == sDto.SerieEntrenadaId.Value);
                    if (dbS == null)
                    {
                        dbEe.Series.Add(CrearSerie(sDto));
                    }
                    else
                    {
                        dbS.NumeroSerie = sDto.NumeroSerie;
                        dbS.Peso = sDto.Peso;
                        dbS.Repeticiones = sDto.Repeticiones;
                        dbS.Completada = sDto.Completada;
                        dbS.FuePR = sDto.FuePR;
                        dbS.RPE = sDto.RPE;
                        dbS.DescansoSegundos = sDto.DescansoSegundos;
                    }
                }
                else
                {
                    dbEe.Series.Add(CrearSerie(sDto));
                }
            }
        }

        private static EjercicioEntrenado CrearEjercicio(EjercicioEntrenadoDto dto)
        {
            var ee = new EjercicioEntrenado
            {
                EjercicioId = dto.EjercicioId,
                Orden = dto.Orden,
                Notas = dto.Notas
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
