using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Avatar_Mod_Administración.Entities
{
    public class PeriodoDto
    {
        [JsonPropertyName("ID_Periodo")]
        public int IdPeriodo { get; set; }

        [JsonPropertyName("Año")]
        [Display(Name = "Año")]
        [Required]
        public int Anno { get; set; }

        [JsonPropertyName("Numero_Periodo")]
        [Display(Name = "Número de periodo")]
        [Required]
        public int NumeroPeriodo { get; set; }

        [JsonPropertyName("Fecha_Inicio")]
        [Display(Name = "Fecha de inicio")]
        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; }

        [JsonPropertyName("Fecha_Fin")]
        [Display(Name = "Fecha final")]
        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaFin { get; set; }


        [JsonPropertyName("Estado")]
        [Display(Name = "Estado (Futuro/Activo/Cerrado)")]
        [Required]
        [RegularExpression("^(Futuro|Activo|Cerrado)$",
            ErrorMessage = "El estado debe ser Futuro, Activo o Cerrado.")]
        public string Estado { get; set; } = "Futuro";


        [JsonIgnore]
        [Display(Name = "Estado calculado")]
        public string EstadoCalculado
        {
            get
            {
                var hoy = DateTime.Today;

                if (FechaInicio.Date > hoy) return "Futuro";
                if (FechaFin.Date < hoy) return "Cerrado";
                return "Activo";
            }
        }
    }
}
