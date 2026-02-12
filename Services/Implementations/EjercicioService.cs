using AnotadorGymAppApi.Context;
using AnotadorGymAppApi.DTOs.Ejercicio;
using AnotadorGymAppApi.DTOs.Musculo;
using AnotadorGymAppApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AnotadorGymAppApi.Services.Implementations
{
    public class EjercicioService : IEjercicioService
    {
        private readonly AppDbContext appDbContext;
        public EjercicioService(AppDbContext dbcontext)
        {
            appDbContext = dbcontext;
        }

        public async Task<IEnumerable<EjercicioDTO>> GetAllEjercicios()
        {
            return await GetEjerciciosQuery().ToListAsync();
        }
        private IQueryable<EjercicioDTO> GetEjerciciosQuery()
        {
            return appDbContext.Ejercicios
        .OrderBy(e => e.Nombre)
        .Select(e => new EjercicioDTO
        {
            EjercicioId = e.EjercicioId,
            Nombre = e.Nombre,
            Descripcion = e.Descripcion,

            GrupoMuscular = e.GrupoMuscular == null
                ? null
                : new GrupoMuscularDTO
                {
                    GrupoMuscularId = e.GrupoMuscular.GrupoMuscularId,
                    Nombre = e.GrupoMuscular.Nombre
                },

            MusculoPrimario = e.MusculoPrimario == null
                ? null
                : new MusculoDTO
                {
                    MusculoId = e.MusculoPrimario.MusculoId,
                    Nombre = e.MusculoPrimario.Nombre
                },

            MusculosSecundarios = e.MusculosSecundarios
                .Select(ms => new MusculoDTO
                {
                    MusculoId = ms.MusculoId,
                    Nombre = ms.Nombre
                })
                .ToList()
        });
        }
        public async Task<EjercicioDTO?> GetEjercicioById(int id)
        {
            return await GetEjerciciosQuery().FirstOrDefaultAsync(e => e.EjercicioId == id);
        }
        public async Task<IEnumerable<EjercicioSimpleDTO>> GetEjerciciosByGrupoMuscular(int grupoMuscularId)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<EjercicioSimpleDTO>> GetEjerciciosByMusculoPrimario(int musculoPrimarioId)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<EjercicioSimpleDTO>> SearchEjercicios(string search)
        {
            throw new NotImplementedException();
        }
    }
}
