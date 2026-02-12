using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AnotadorGymAppApi.Context;
using Microsoft.EntityFrameworkCore;
using AnotadorGymAppApi.DTOs.Ejercicio;
using AnotadorGymAppApi.Services.Implementations;
using AnotadorGymAppApi.Services.Interfaces;

namespace AnotadorGymAppApi.Controllers
{
    [Route("api/ejercicios")]
    [ApiController]
    public class EjercicioController : ControllerBase
    {
        private readonly IEjercicioService ejercicioService;
        public EjercicioController(IEjercicioService ejercicioService)
        {
            this.ejercicioService = ejercicioService;
        }

        /// <summary>
        /// Obtiene todos los ejercicios registrados.
        /// </summary>        
        /// <returns>Lista de ejercicios</returns>
        /// <response code="200">Ejercicios encontrados</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]        
        public async Task<ActionResult<IEnumerable<EjercicioDTO>>> GetEjercicios()
        {
            var ejercicios = await ejercicioService.GetAllEjercicios();
            return Ok(ejercicios);
        }        
    }
}
