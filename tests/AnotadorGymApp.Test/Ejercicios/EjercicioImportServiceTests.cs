using AnotadorGymAppApi.Features.Ejercicios;
using AnotadorGymAppApi.Features.Ejercicios.DTOs;
using AnotadorGymAppApi.Features.Common.Validation;
using AnotadorGymAppApi.Infrastructure.Cache;
using AnotadorGymApp.Test.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using AnotadorGymAppApi.Infrastructure.Context;
using System;

namespace AnotadorGymApp.Test.Ejercicios
{
    public class EjercicioImportServiceTests
    {
        [Fact]
        public async Task ImportarEjerciciosDesdeJsonAsync_DeberiaCrearEntidadesYCambiarCache_CuandoDatosValidos()
        {
            // Arrange
            var db = await DbContextHelper.AppDbContextPrueba();

            var mockValidator = new Mock<IJsonFileValidator>();
            var mockCache = new Mock<ICacheService>();
            mockCache.Setup(c => c.DeleteAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            var logger = NullLogger<EjercicioImportService>.Instance;

            var service = new EjercicioImportService(db, logger, mockValidator.Object, mockCache.Object);

            var ejercicios = new List<EjercicioDto>
            {
                new EjercicioDto
                {
                    Nombre = "Press Banca",
                    GrupoMuscular = new GrupoMuscularDTO { Nombre = "Pecho" },
                    MusculoPrimario = new MusculoDTO { Nombre = "Pectoral Mayor" }
                }
            };

            // Configurar el mock del validador para simular un archivo JSON válido
            mockValidator.Setup(v => v.ValidateJsonFileAsync<System.Collections.Generic.List<EjercicioDto>>(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>()))
                .ReturnsAsync((true, ejercicios, (string?)null));

            // Act
            var result = await service.ImportarEjerciciosDesdeJsonAsync(ejercicios);

            // Assert
            result.Errores.Should().BeEmpty();
            result.EjerciciosCreados.Should().Be(1);
            result.GruposMuscularesCreados.Should().BeGreaterThanOrEqualTo(1);
            result.MusculosCreados.Should().BeGreaterThanOrEqualTo(1);
            mockCache.Verify(c => c.DeleteAsync("Ejercicios.json"), Times.Once);

            // Además, verificar que los datos se hayan persistido en la BD
            db.Ejercicios.Should().ContainSingle(e => e.Nombre == "Press Banca");
            db.GrupoMusculares.Should().Contain(g => g.Nombre.ToLower().Contains("pecho"));
            db.Musculos.Should().Contain(m => m.Nombre.ToLower().Contains("pectoral"));
        }

        [Fact]
        public async Task ImportarEjerciciosDesdeJsonAsync_DeberiaRegistrarErrorGeneral_CuandoCacheDeleteFalla()
        {
            // Arrange
            var db = await DbContextHelper.AppDbContextPrueba();

            var mockValidator = new Mock<IJsonFileValidator>();
            var mockCache = new Mock<ICacheService>();
            mockCache.Setup(c => c.DeleteAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException("Cache failure"));

            var logger = NullLogger<EjercicioImportService>.Instance;

            var service = new EjercicioImportService(db, logger, mockValidator.Object, mockCache.Object);

            var ejercicios = new List<EjercicioDto>
            {
                new EjercicioDto
                {
                    Nombre = "Sentadilla",
                    GrupoMuscular = new GrupoMuscularDTO { Nombre = "Piernas" },
                    MusculoPrimario = new MusculoDTO { Nombre = "Cuádriceps" }
                }
            };

            // Configurar el mock del validador para simular un archivo JSON válido
            mockValidator.Setup(v => v.ValidateJsonFileAsync<System.Collections.Generic.List<EjercicioDto>>(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>()))
                .ReturnsAsync((true, ejercicios, (string?)null));

            // Act
            var result = await service.ImportarEjerciciosDesdeJsonAsync(ejercicios);

            // Assert
            result.Errores.Should().NotBeEmpty();
            result.Errores.Should().Contain(e => e.Mensaje != null && e.Mensaje.StartsWith("Error general:"));
            mockCache.Verify(c => c.DeleteAsync("Ejercicios.json"), Times.Once);

            // Asegurar que cuando DeleteAsync del caché falla, la transacción se revierte y no se persisten los ejercicios
            db.Ejercicios.Should().NotContain(e => e.Nombre == "Sentadilla");
        }

        [Fact]
        public async Task ImportarEjerciciosDesdeArchivoAsync_DeberiaMarcarFalloCritico_CuandoArchivoJsonNoValido()
        {
            // Preparación
            var db = await DbContextHelper.AppDbContextPrueba();

            var mockValidator = new Mock<IJsonFileValidator>();
            var mockCache = new Mock<ICacheService>();
            var logger = NullLogger<EjercicioImportService>.Instance;

            // Simular que el validador indica que el JSON no es válido
            var errorMensaje = "Formato inválido";
            mockValidator.Setup(v => v.ValidateJsonFileAsync<System.Collections.Generic.List<EjercicioDto>>(It.IsAny<IFormFile>()))
                .ReturnsAsync((false, (List<EjercicioDto>?)null, errorMensaje));

            var service = new EjercicioImportService(db, logger, mockValidator.Object, mockCache.Object);

            // Crear un IFormFile simulado con nombre de archivo para que el método no lance NullReference
            var mockFormFile = new Mock<IFormFile>();
            mockFormFile.Setup(f => f.FileName).Returns("ejercicios.json");

            // Ejecución
            var result = await service.ImportarEjerciciosDesdeArchivoAsync(mockFormFile.Object);

            // Verificar
            result.FalloCritico.Should().BeTrue();
            result.Errores.Should().Contain(e => e.Mensaje != null && e.Mensaje.Contains(errorMensaje));
            // No debe llamarse a DeleteAsync del caché
            mockCache.Verify(c => c.DeleteAsync(It.IsAny<string>()), Times.Never);
            // No deben haberse persistido ejercicios
            db.Ejercicios.Should().BeEmpty();
        }
        // Clases DTO auxiliares para evitar referenciar los namespaces internos de DTOs en el código de prueba
        private class GrupoMuscularDTO : AnotadorGymAppApi.Features.Ejercicios.DTOs.GrupoMuscularDTO { }
        private class MusculoDTO : AnotadorGymAppApi.Features.Ejercicios.DTOs.MusculoDTO { }
    }
}
