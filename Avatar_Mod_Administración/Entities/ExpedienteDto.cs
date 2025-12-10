using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Avatar_Mod_Administración.Entities
{
    public class ExpedienteDto
    {
        [JsonPropertyName("Numero_Identificacion")]
        [Display(Name = "Número de identificación")]
        [Required]
        [StringLength(20)]
        public string NumeroIdentificacion { get; set; } = string.Empty;

        [JsonPropertyName("Tipo_Identificacion")]
        [Display(Name = "Tipo de identificación")]
        [Required]
        public string TipoIdentificacion { get; set; } = string.Empty;

        [JsonPropertyName("Email")]
        [Display(Name = "Correo electrónico")]
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("Nombre_Completo")]
        [Display(Name = "Nombre completo")]
        [Required]
        [StringLength(80)]
        public string NombreCompleto { get; set; } = string.Empty;

        [JsonPropertyName("Fecha_Nacimiento")]
        [Display(Name = "Fecha de nacimiento")]
        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaNacimiento { get; set; }

        [JsonPropertyName("ID_Provincia")]
        [Display(Name = "Provincia")]
        [Required]
        public int IdProvincia { get; set; }

        [JsonPropertyName("ID_Canton")]
        [Display(Name = "Cantón")]
        [Required]
        public int IdCanton { get; set; }

        [JsonPropertyName("DistritoID")]
        [Display(Name = "Distrito")]
        [Required]
        public int IdDistrito { get; set; }

        [JsonPropertyName("Telefonos")]
        [Display(Name = "Teléfonos")]
        [Required]
        [StringLength(50)]
        public string Telefonos { get; set; } = string.Empty;
    }
}
