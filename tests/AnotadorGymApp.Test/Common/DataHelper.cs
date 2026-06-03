using AnotadorGymAppApi.Domain.Entities.Ejercicio;
using AnotadorGymAppApi.Domain.Entities.Rutina;
using AnotadorGymAppApi.Features.Ejercicios.DTOs;
using AnotadorGymAppApi.Features.Rutinas.DTOs;
using AnotadorGymAppApi.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotadorGymApp.Test.Common
{
    public class DataHelper
    {
        public static List<Ejercicio> ObtenerEjerciciosFake(AppDbContext? dbContext)
        {
            #region Datos de Ejercicios
            var grupoPecho = new GrupoMuscular
            {
                GrupoMuscularId = 1,
                Nombre = "Pecho"
            };

            var grupoPiernas = new GrupoMuscular
            {
                GrupoMuscularId = 2,
                Nombre = "Piernas"
            };

            var pectoralMayor = new Musculos
            {
                MusculoId = 1,
                Nombre = "Pectoral Mayor"
            };

            var cuadriceps = new Musculos
            {
                MusculoId = 2,
                Nombre = "Cuádriceps"
            };

            var triceps = new Musculos
            {
                MusculoId = 3,
                Nombre = "Tríceps"
            };

            var gluteos = new Musculos
            {
                MusculoId = 4,
                Nombre = "Glúteos"
            };

            var ejerciciosFake = new List<Ejercicio>
            {
                new Ejercicio
                {
                    EjercicioId = 1,
                    Nombre = "Press Banca",
                    Descripcion = "Ejercicio compuesto para pecho",
                    UrlVideo = "https://youtube.com/press-banca",

                    GrupoMuscularId = grupoPecho.GrupoMuscularId,
                    GrupoMuscular = grupoPecho,

                    MusculoPrimarioId = pectoralMayor.MusculoId,
                    MusculoPrimario = pectoralMayor,

                    MusculosSecundarios =
                    {
                        triceps
                    }
                },
                new Ejercicio
                {
                    EjercicioId = 2,
                    Nombre = "Sentadilla",
                    Descripcion = "Ejercicio compuesto para piernas",
                    UrlVideo = "https://youtube.com/sentadilla",

                    GrupoMuscularId = grupoPiernas.GrupoMuscularId,
                    GrupoMuscular = grupoPiernas,

                    MusculoPrimarioId = cuadriceps.MusculoId,
                    MusculoPrimario = cuadriceps,

                    MusculosSecundarios =
                    {
                        gluteos
                    }
                }
            };

            #endregion

            if (dbContext != null)
            {
                dbContext.Ejercicios.AddRange(ejerciciosFake);
                dbContext.SaveChanges();
            }

            return ejerciciosFake;
        }
        public static List<Rutina> ObtenerRutinasFake(AppDbContext? dbContext)
        {
            var pesoMuerto = new Ejercicio
            {
                EjercicioId = 1,
                Nombre = "Peso Muerto"
            };

            var sentadilla = new Ejercicio
            {
                EjercicioId = 2,
                Nombre = "Sentadilla"
            };

            var rutinasFake = new List<Rutina>()
            {
                new Rutina
                {
                    RutinaId = 1,
                    Nombre = "Rutina A",
                    Semanas = new List<RutinaSemana>
                    {
                        new RutinaSemana
                        {
                            RutinaSemanaId = 1,
                            Dias = new List<RutinaDia>
                            {
                                new RutinaDia
                                {
                                    RutinaDiaId = 2,
                                    Ejercicios = new List<RutinaEjercicio>
                                    {
                                        new RutinaEjercicio
                                        {
                                            RutinaEjercicioId = 3,
                                            Ejercicio = pesoMuerto
                                        },
                                        new RutinaEjercicio
                                        {
                                            RutinaEjercicioId = 4,
                                            Ejercicio = sentadilla
                                        }
                                    }
                                },
                                new RutinaDia
                                {
                                    RutinaDiaId = 3,
                                    Ejercicios = new List<RutinaEjercicio>
                                    {
                                        new RutinaEjercicio
                                        {
                                            RutinaEjercicioId = 5,
                                            Ejercicio = pesoMuerto
                                        },
                                        new RutinaEjercicio
                                        {
                                            RutinaEjercicioId = 6,
                                            Ejercicio = sentadilla
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                new Rutina
                {
                    RutinaId = 2,
                    Nombre = "Rutina B",
                    Semanas = new List<RutinaSemana>
                    {
                        new RutinaSemana
                        {
                            RutinaSemanaId = 2,
                            Dias = new List<RutinaDia>
                            {
                                new RutinaDia
                                {
                                    RutinaDiaId = 4,
                                    Ejercicios = new List<RutinaEjercicio>
                                    {
                                        new RutinaEjercicio
                                        {
                                            RutinaEjercicioId = 7,
                                            Ejercicio = pesoMuerto
                                        },
                                        new RutinaEjercicio
                                        {
                                            RutinaEjercicioId = 8,
                                            Ejercicio = sentadilla
                                        }
                                    }
                                },
                                new RutinaDia
                                {
                                    RutinaDiaId = 5,
                                    Ejercicios = new List<RutinaEjercicio>
                                    {
                                        new RutinaEjercicio
                                        {
                                            RutinaEjercicioId = 9,
                                            Ejercicio = pesoMuerto
                                        },
                                        new RutinaEjercicio
                                        {
                                            RutinaEjercicioId = 10,
                                            Ejercicio = sentadilla
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            };

            if (dbContext != null)
            {
                dbContext.Rutinas.AddRange(rutinasFake);
                dbContext.SaveChanges();
            }

            return rutinasFake;
        }
        public static List<RutinaDto> CrearRutinasDtoDesdeEntities(List<Rutina> rutina)
        {
            var rutinasDtoEnCacheDtoJson = rutina.Select(r => new RutinaDto
            {
                RutinaId = r.RutinaId,
                Nombre = r.Nombre,
                Descripcion = r.Descripcion,
                Semanas = r.Semanas.Select(s => new RutinaSemanaDto
                {
                    RutinaSemanaId = s.RutinaSemanaId,
                    Dias = s.Dias.Select(d => new RutinaDiaDto
                    {
                        RutinaDiaId = d.RutinaDiaId,
                        Ejercicios = d.Ejercicios.Select(e => new RutinaEjercicioDto
                        {
                            RutinaEjercicioId = e.RutinaEjercicioId,
                            Ejercicio = new EjercicioSimpleDTO
                            {
                                EjercicioId = e.EjercicioId,
                                Nombre = e.Ejercicio.Nombre,
                                Descripcion = e.Ejercicio.Descripcion,
                                UrlVideo = e.Ejercicio.UrlVideo
                            },
                        }).ToList()
                    }).ToList()
                }).ToList(),
            }).ToList();
            return rutinasDtoEnCacheDtoJson;
        }

    }
}
