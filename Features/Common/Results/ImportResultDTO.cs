using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AnotadorGymAppApi.Features.Common.Results
{
    public class ImportResultDTO
    {
        [Description("Operacion No Iniciada por Fallo Critico")]
        public bool FalloCritico { get; set; }

        [Description("Número de ejercicios nuevos creados")]
        public int EjerciciosCreados { get; set; }

        [Description("Número de rutinas nuevas creadas")]
        public int RutinasCreadas { get; set; }

        [Description("Número de ejercicios actualizados")]
        public int EjerciciosActualizados { get; set; }

        [Description("Número de músculos nuevos creados")]
        public int MusculosCreados { get; set; }

        [Description("Número de ejercicios omitidos (ya existían)")]
        public int EjerciciosOmitidos { get; set; }

        [Description("Número de grupos musculares nuevos creados")]
        public int GruposMuscularesCreados { get; set; }

        [Description("Lista de errores durante la importación")]
        public List<ImportErrorDTO> Errores { get; set; } = new();

        [Description("Lista de advertencias durante la importación")]
        public List<string> Advertencias { get; set; } = new();

        [Description("Duración total de la operación de importación")]
        public TimeSpan Duracion { get; set; }

        [Description("Total de ejercicios procesados (intentados)")]
        public int TotalProcesados { get; set; }
        
        [JsonIgnore]
        [Description("Indica si hubo errores durante la importación")]
        public bool TieneErrores => Errores.Any();

        [JsonIgnore]
        [Description("Indica si hubo advertencias durante la importación")]
        public bool TieneAdvertencias => Advertencias.Any();

        [JsonIgnore]
        [Description("Total de ejercicios afectados (creados + actualizados)")]
        public int TotalEjerciciosAfectados => EjerciciosCreados + EjerciciosActualizados;

        [JsonIgnore]
        [Description("Total de registros nuevos (ejercicios + músculos + grupos)")]
        public int TotalRegistrosCreados => EjerciciosCreados + MusculosCreados + GruposMuscularesCreados;

        [JsonIgnore]
        [Description("Porcentaje de éxito en la importación")]
        public double PorcentajeExito => TotalProcesados > 0
            ? TotalEjerciciosAfectados * 100.0 / TotalProcesados
            : 0;

        [JsonIgnore]
        [Description("Duración en milisegundos")]
        public double DuracionMilisegundos => Duracion.TotalMilliseconds;

        [JsonIgnore]
        [Description("Duración formateada para mostrar")]
        public string DuracionFormateada => FormatearDuracion(Duracion);

        private static string FormatearDuracion(TimeSpan duracion)
        {
            if (duracion.TotalHours >= 1)
                return $"{duracion.TotalHours:F2} horas";
            if (duracion.TotalMinutes >= 1)
                return $"{duracion.TotalMinutes:F2} minutos";
            if (duracion.TotalSeconds >= 1)
                return $"{duracion.TotalSeconds:F2} segundos";

            return $"{duracion.TotalMilliseconds:F0} ms";
        }

        public ImportResultDTO() { }
    }
}
