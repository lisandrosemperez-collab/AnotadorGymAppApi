using AnotadorGymApp.Test.Common;
using AnotadorGymAppApi.Features.Rutinas;
using AnotadorGymAppApi.Features.Rutinas.DTOs;
using AnotadorGymAppApi.Features.Ejercicios.DTOs;
using AnotadorGymAppApi.Infrastructure.Context;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AnotadorGymApp.Test.Rutinas
{
    public class RutinaImportServiceTests
    {
        [Fact]
        public async Task ImportarRutinasDesdeJsonAsync_DeberiaCrearRutina_CuandoDatosValidos()
        {
            var context = await DbContextHelper.AppDbContextPrueba();
            // Agregamos ejercicios necesarios a la base de datos
            DataHelper.ObtenerEjerciciosFake(context);

            var logger = NullLogger<RutinaImportService>.Instance;
            var mockValidator = new Mock<AnotadorGymAppApi.Features.Common.Validation.IJsonFileValidator>();

            var service = new RutinaImportService(context, logger, mockValidator.Object);

            var dto = new RutinaDto
            {
                Nombre = "Rutina Importada",
                Descripcion = "Descripcion",
                Semanas = new List<RutinaSemanaDto>
                {
                    new RutinaSemanaDto
                    {
                        Dias = new List<RutinaDiaDto>
                        {
                            new RutinaDiaDto
                            {
                                Ejercicios = new List<RutinaEjercicioDto>
                                {
                                    new RutinaEjercicioDto
                                    {
                                        Ejercicio = new EjercicioSimpleDTO { Nombre = "Press Banca" },
                                        Series = new List<RutinaSerieDto>
                                        {
                                            new RutinaSerieDto { Repeticiones = 8, Descanso = "00:01:00", Tipo = AnotadorGymAppApi.Domain.Entities.Rutina.TipoSerie.Normal }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var result = await service.ImportarRutinasDesdeJsonAsync(new List<RutinaDto> { dto });
            result.FalloCritico.Should().BeFalse();
            result.RutinasCreadas.Should().Be(1);

            // Verificamos que la rutina fue persistida
            context.Rutinas.Should().Contain(r => r.Nombre == "Rutina Importada");
        }

        [Fact]
        public async Task ImportarRutinasDesdeJsonAsync_DeberiaFallar_SiEjercicioNoExiste()
        {
            var context = await DbContextHelper.AppDbContextPrueba();
            // No agregamos ejercicios a la DB para forzar error

            var logger = NullLogger<RutinaImportService>.Instance;
            var mockValidator = new Mock<AnotadorGymAppApi.Features.Common.Validation.IJsonFileValidator>();
            var service = new RutinaImportService(context, logger, mockValidator.Object);

            var dto = new RutinaDto
            {
                Nombre = "Rutina Fallida",
                Semanas = new List<RutinaSemanaDto>
                {
                    new RutinaSemanaDto
                    {
                        Dias = new List<RutinaDiaDto>
                        {
                            new RutinaDiaDto
                            {
                                Ejercicios = new List<RutinaEjercicioDto>
                                {
                                    new RutinaEjercicioDto
                                    {
                                        Ejercicio = new EjercicioSimpleDTO { Nombre = "Ejercicio Inexistente" },
                                        Series = new List<RutinaSerieDto>
                                        {
                                            new RutinaSerieDto { Repeticiones = 5, Descanso = "00:01:00", Tipo = AnotadorGymAppApi.Domain.Entities.Rutina.TipoSerie.Normal }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var result = await service.ImportarRutinasDesdeJsonAsync(new List<RutinaDto> { dto });

            result.FalloCritico.Should().BeTrue();
            result.Errores.Should().NotBeEmpty();
            context.Rutinas.Should().BeEmpty();
        }

        [Fact]
        public async Task ImportarRutinasDesdeJsonAsync_DeberiaDetectarDescansoInvalido_YNoPersistir()
        {
            var context = await DbContextHelper.AppDbContextPrueba();
            DataHelper.ObtenerEjerciciosFake(context);

            var logger = NullLogger<RutinaImportService>.Instance;
            var mockValidator = new Mock<AnotadorGymAppApi.Features.Common.Validation.IJsonFileValidator>();
            var service = new RutinaImportService(context, logger, mockValidator.Object);

            var dto = new RutinaDto
            {
                Nombre = "Rutina Descanso Invalido",
                Semanas = new List<RutinaSemanaDto>
                {
                    new RutinaSemanaDto
                    {
                        Dias = new List<RutinaDiaDto>
                        {
                            new RutinaDiaDto
                            {
                                Ejercicios = new List<RutinaEjercicioDto>
                                {
                                    new RutinaEjercicioDto
                                    {
                                        Ejercicio = new EjercicioSimpleDTO { Nombre = "Press Banca" },
                                        Series = new List<RutinaSerieDto>
                                        {
                                            new RutinaSerieDto { Repeticiones = 10, Descanso = "invalid-time", Tipo = AnotadorGymAppApi.Domain.Entities.Rutina.TipoSerie.Normal }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var result = await service.ImportarRutinasDesdeJsonAsync(new List<RutinaDto> { dto });

            result.FalloCritico.Should().BeTrue();
            result.Errores.Should().NotBeEmpty();
            context.Rutinas.Should().BeEmpty();
        }

        [Fact]
        public async Task ImportarRutinasDesdeJsonAsync_DeberiaRetornarFalloSiListaNulaOVacia()
        {
            var context = await DbContextHelper.AppDbContextPrueba();
            var logger = NullLogger<RutinaImportService>.Instance;
            var mockValidator = new Mock<AnotadorGymAppApi.Features.Common.Validation.IJsonFileValidator>();
            var service = new RutinaImportService(context, logger, mockValidator.Object);

            var resultNull = await service.ImportarRutinasDesdeJsonAsync(null);
            resultNull.FalloCritico.Should().BeTrue();
            resultNull.Errores.Should().NotBeEmpty();

            var resultEmpty = await service.ImportarRutinasDesdeJsonAsync(new List<RutinaDto>());
            resultEmpty.FalloCritico.Should().BeTrue();
            resultEmpty.Errores.Should().NotBeEmpty();
        }
    }
}
