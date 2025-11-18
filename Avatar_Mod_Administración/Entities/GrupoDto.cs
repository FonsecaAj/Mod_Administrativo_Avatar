using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Avatar_Mod_Administración.Entities
{
    public class GrupoDto
    {
        [JsonPropertyName("ID_Grupo")]
        public int IdGrupo { get; set; }

        [JsonPropertyName("Numero_Grupo")]
        [Display(Name = "Número de grupo")]
        [Required(ErrorMessage = "El número de grupo es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El número de grupo debe ser mayor a cero.")]
        public int NumeroGrupo { get; set; }

        [JsonPropertyName("ID_Curso")]
        [Display(Name = "Curso")]
        [Required(ErrorMessage = "El curso es obligatorio.")]
        public int IdCurso { get; set; }

        [JsonPropertyName("ID_Profesor")]
        [Display(Name = "Profesor")]
        [Required(ErrorMessage = "El profesor es obligatorio.")]
        public int IdProfesor { get; set; }

        [JsonPropertyName("Horario")]
        [Display(Name = "Horario")]
        [Required(ErrorMessage = "El horario es obligatorio.")]
        [RegularExpression(@"^\d{2}:\d{2}\s*-\s*\d{2}:\d{2}$",
            ErrorMessage = "El horario debe tener el formato HH:MM")]
        public string Horario { get; set; } = string.Empty;

        [JsonPropertyName("ID_Periodo")]
        [Display(Name = "Periodo")]
        [Required(ErrorMessage = "El periodo es obligatorio.")]
        public int IdPeriodo { get; set; }

        [JsonPropertyName("CursoNombre")]
        [Display(Name = "Curso")]
        public string? CursoNombre { get; set; }

        [JsonPropertyName("ProfesorNombre")]
        [Display(Name = "Profesor")]
        public string? ProfesorNombre { get; set; }

        [JsonPropertyName("PeriodoNombre")]
        [Display(Name = "Periodo")]
        public string? PeriodoNombre { get; set; }
    }
}
