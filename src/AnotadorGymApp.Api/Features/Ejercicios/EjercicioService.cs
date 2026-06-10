using AnotadorGymAppApi.Domain.Entities.Ejercicio;
using AnotadorGymAppApi.Features.Common.Pagination;
using AnotadorGymAppApi.Features.Ejercicios.DTOs;
using AnotadorGymAppApi.Infrastructure.Cache;
using AnotadorGymAppApi.Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using AnotadorGymAppApi.Features.Common.Tools;
using AnotadorGymAppApi.Features.Ejercicios.Results;
using AnotadorGymApp.Api.Features.Common.Tools;
using System.Diagnostics;

namespace AnotadorGymAppApi.Features.Ejercicios
{
    public class EjercicioService : IEjercicioService
    {
        private readonly AppDbContext appDbContext;
        private readonly ICacheService cacheService;
        private const string CACHE_KEY = "Ejercicios.json";
        public EjercicioService(AppDbContext dbcontext , ICacheService cacheService)
        {
            this.cacheService = cacheService;
            appDbContext = dbcontext;
        }
        private IQueryable<Ejercicio> BaseQuery()
        {
            return appDbContext.Ejercicios.AsNoTracking();
        }
        private IQueryable<EjercicioDto> ProjectToDto(IQueryable<Ejercicio> query)
        {
            return query.Select(e => new EjercicioDto
            {
                Nombre = e.Nombre,
                EjercicioId = e.EjercicioId,
                Descripcion = e.Descripcion,
                UrlVideo = e.UrlVideo,
                GrupoMuscular = e.GrupoMuscular == null
                    ? null
                    : new GrupoMuscularDTO
                    {
                        Nombre = e.GrupoMuscular.Nombre
                    },
                MusculoPrimario = e.MusculoPrimario == null
                    ? null
                    : new MusculoDTO
                    {
                        Nombre = e.MusculoPrimario.Nombre
                    },
                MusculosSecundarios = e.MusculosSecundarios
                    .Select(ms => new MusculoDTO
                    {
                        Nombre = ms.Nombre
                    })
                    .ToList()
            });
        }
        
        #region Gets
        public async Task<(List<EjercicioDto>, bool)> GetAllEjercicios()
        {
            var desdeCache = false;
            var ejerciciosCacheJson = await cacheService.GetAsync(CACHE_KEY);            

            if (!string.IsNullOrWhiteSpace(ejerciciosCacheJson))
            {
                try
                {
                    // Intentamos deserializar; si falla, se lanzará una excepción que atrapamos para fallback a BD
                    var datos = DeserealizarCache.DeserializarCache<EjercicioDto>(ejerciciosCacheJson);
                    desdeCache = true;
                    return (datos, desdeCache);

                }
                catch (JsonException)
                {
                    // JSON inválido: fallback a BD y refrescar caché
                    var datos = await CargarEjerciciosDesdeDbYRefrescarCacheAsync();
                    desdeCache = false;
                    return (datos, desdeCache);
                }
                catch (Exception)
                {
                    // Cualquier otro error al deserializar: fallback a BD
                    var datos = await CargarEjerciciosDesdeDbYRefrescarCacheAsync();
                    desdeCache = false;
                    return (datos, desdeCache);
                }
            }            
            
            var result = await CargarEjerciciosDesdeDbYRefrescarCacheAsync();                       

            Debug.WriteLine($"Result Count: {result.Count}");                        

            return (result, desdeCache);
        }
        public async Task<(List<EjercicioDto?>, int,bool)> GetEjercicios(PaginationParams pagination)
        {
            if (pagination.Page < 1) pagination.Page = 1;
            if (pagination.PageSize <= 0) pagination.PageSize = 10;
            if (pagination.PageSize > 100) pagination.PageSize = 100;

            var cache = await cacheService.GetAsync(CACHE_KEY);            
            List<EjercicioDto> todosEjercicios;
            bool desdeCache = false;

            if (!string.IsNullOrEmpty(cache))
            {
                try
                {
                    todosEjercicios = DeserealizarCache.DeserializarCache<EjercicioDto>(cache);
                    desdeCache = true;
                }
                catch (JsonException)
                {
                    // JSON inválido: fallback a BD y refrescar caché                    
                    todosEjercicios = await CargarEjerciciosDesdeDbYRefrescarCacheAsync();                    
                    desdeCache = false;
                }
                catch (Exception)
                {
                    // Cualquier otro error al deserializar: fallback a BD
                    todosEjercicios = await CargarEjerciciosDesdeDbYRefrescarCacheAsync();                    
                    desdeCache = false;
                }
            }
            else
            {
                todosEjercicios = await CargarEjerciciosDesdeDbYRefrescarCacheAsync();                
                desdeCache = false;
            }

            todosEjercicios = FiltrarEjercicios(todosEjercicios, pagination);

            var totalCount = todosEjercicios.Count();

            var paginados = PaginarEjercicios(todosEjercicios, pagination);

            return (paginados, totalCount, desdeCache);
        }        
        public async Task<(EjercicioDto?, bool)> GetPorId(int id)
        {
            var ejerciciosCache = await cacheService.GetAsync(CACHE_KEY);
            var desdeCache = false;

            if (!string.IsNullOrWhiteSpace(ejerciciosCache))
            {
                try
                {
                    var data = DeserealizarCache.DeserializarCache<EjercicioDto>(ejerciciosCache);
                    desdeCache = true;
                    var ejercicio = data.FirstOrDefault(e => e.EjercicioId == id);
                    return (ejercicio, desdeCache);
                }               
                catch (Exception)
                {
                    // Cualquier otro error al deserializar: fallback a BD
                    var data = await CargarEjerciciosDesdeDbYRefrescarCacheAsync();
                    desdeCache = false;
                    var ejercicio = data.FirstOrDefault(e => e.EjercicioId == id);
                    return (ejercicio, desdeCache);
                }
            }
            var todos = await CargarEjerciciosDesdeDbYRefrescarCacheAsync();

            return (
                todos.FirstOrDefault(e => e.EjercicioId == id),
                false
            );
        }

        #endregion

        #region Sets        
        public async Task<bool> EliminarEjercicioAsync(int ejercicioId)
        {
            var ejercicio = await appDbContext.Ejercicios
                .FirstOrDefaultAsync(e => e.EjercicioId == ejercicioId);

            if (ejercicio == null) return false;

            appDbContext.Ejercicios.Remove(ejercicio);
            await appDbContext.SaveChangesAsync();

            return true;
        }
        public async Task<EjercicioSimpleDTO> ActualizarEjercicioAsync(int id, EjercicioSimpleDTO dto)
        {
            var ejercicio = await appDbContext.Ejercicios.FindAsync(id);

            if (ejercicio == null)
                return null;

            AplicarActualizacion(ejercicio, dto);

            await appDbContext.SaveChangesAsync();                

            await cacheService.DeleteAsync("Ejercicios.json");
            
            return new EjercicioSimpleDTO
            {
                EjercicioId = ejercicio.EjercicioId,
                Nombre = ejercicio.Nombre,
                Descripcion = ejercicio.Descripcion,
                UrlVideo = ejercicio.UrlVideo
            };


        }
        public async Task<ActualizarResult> ActualizarEjerciciosAsync(List<EjercicioSimpleDTO> dtos)
        {
            var result = new ActualizarResult();

            var porId = dtos.Where(x => x.EjercicioId > 0).ToList();            

            // MATCH POR ID (PRIORIDAD)

            if (porId.Any())
            {
                var ids = porId.Select(x => x.EjercicioId).ToList();

                var ejerciciosById = await appDbContext.Ejercicios
                    .Where(x => ids.Contains(x.EjercicioId))
                    .ToListAsync();

                var dict = ejerciciosById.ToDictionary(x => x.EjercicioId);

                foreach (var dto in porId)
                {
                    if (!dict.TryGetValue(dto.EjercicioId, out var ex))
                    {
                        result.NotFound++;
                        continue;
                    }                    
                    
                    AplicarActualizacion(ex, dto);
                    result.UpdatedById++;                    
                }
            }

            var porNombre = dtos.Where(x => x.EjercicioId == 0).ToList();

            // FALLBACK POR NOMBRE
            if (porNombre.Any())
            {

                var nombresNormalizados = porNombre
                    .Select(x => StringNormalize.Normalize(x.Nombre))
                    .ToList();

                var ejerciciosByName = await appDbContext.Ejercicios                
                    .ToListAsync();

                ejerciciosByName = ejerciciosByName
                    .Where(x => nombresNormalizados.Contains(StringNormalize.Normalize(x.Nombre)))
                    .ToList();

                var dictName = ejerciciosByName.ToDictionary(
                    x => StringNormalize.Normalize(x.Nombre)
                );

                foreach (var dto in porNombre)
                {
                    var dtoName = StringNormalize.Normalize(dto.Nombre);
                    
                    if (!dictName.TryGetValue(dtoName, out var ejercicio))
                    {
                        result.NotFound++;
                        continue;
                    }                    
                    AplicarActualizacion(ejercicio, dto);
                    result.UpdatedByName++;
                }
            }

            await appDbContext.SaveChangesAsync();
            
            await cacheService.DeleteAsync("Ejercicios.json");

            return result;
        }
        private void AplicarActualizacion(Ejercicio ejercicio, EjercicioSimpleDTO dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.Nombre))
                ejercicio.Nombre = dto.Nombre;

            if (!string.IsNullOrWhiteSpace(dto.Descripcion))
                ejercicio.Descripcion = dto.Descripcion;

            if (!string.IsNullOrWhiteSpace(dto.UrlVideo))
                ejercicio.UrlVideo = dto.UrlVideo;
        }

        #endregion

        private List<EjercicioDto> FiltrarEjercicios(
            List<EjercicioDto> ejercicios,
            PaginationParams pagination)
        {
            if (string.IsNullOrWhiteSpace(pagination.Nombre))
                return ejercicios;

            return ejercicios
                .Where(e =>
                    e.Nombre.Contains(
                        pagination.Nombre,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private List<EjercicioDto> PaginarEjercicios(
            List<EjercicioDto> ejercicios,
            PaginationParams pagination)
        {
            return ejercicios
                .OrderBy(e => e.Nombre)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();
        }

        private async Task<List<EjercicioDto>> CargarEjerciciosDesdeDbYRefrescarCacheAsync()
        {
            // Obtener desde BD y proyectar a DTO
            var todosEjercicios = await ProjectToDto(BaseQuery()).ToListAsync();

            // Intentar refrescar la caché; en caso de fallo, solo lo registramos y devolvemos los datos
            try
            {
                var json = JsonSerializer.Serialize(todosEjercicios);
                await cacheService.SetAsync(CACHE_KEY, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al actualizar caché ({CACHE_KEY}): {ex.Message}");
            }

            return todosEjercicios;
        }

    }
}
