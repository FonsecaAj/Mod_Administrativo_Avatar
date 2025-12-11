using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Avatar_Mod_Administración.Entities
{
    public class ProfesorDto
    {
        [JsonPropertyName("ID_Profesor")]
        public int IdProfesor { get; set; }

        [Display(Name = "Número de identificación")]
        [JsonPropertyName("Numero_Identificacion")]
        [Required(ErrorMessage = "El número de identificación es requerido.")]
        [StringLength(20, ErrorMessage = "La identificación no puede superar los 20 caracteres.")]
        public string NumeroIdentificacion { get; set; } = string.Empty;

        [Display(Name = "Tipo de identificación")]
        [JsonPropertyName("Tipo_Identificacion")]
        [Required(ErrorMessage = "El tipo de identificación es requerido.")]
        [StringLength(20)]
        public string TipoIdentificacion { get; set; } = string.Empty;

        [JsonPropertyName("Email")]
        [Required(ErrorMessage = "El correo es requerido.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [RegularExpression(@"^[^@\s]+@cuc\.ac\.cr$",
            ErrorMessage = "El correo debe pertenecer al dominio cuc.ac.cr")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Nombre completo")]
        [JsonPropertyName("Nombre_Completo")]
        [Required(ErrorMessage = "El nombre completo es requerido.")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúñÑ ]+$",
            ErrorMessage = "El nombre solo puede contener letras y espacios.")]
        [StringLength(100)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Display(Name = "Fecha de nacimiento")]
        [JsonPropertyName("Fecha_Nacimiento")]
        [Required(ErrorMessage = "La fecha de nacimiento es requerida.")]
        [DataType(DataType.Date)]
        public DateTime FechaNacimiento { get; set; }

        [Display(Name = "Teléfono")]
        [JsonPropertyName("Telefono")]
        [Required(ErrorMessage = "El teléfono es requerido.")]
        [RegularExpression(@"^[245678][0-9]{7}$",
            ErrorMessage = "Ingrese un número telefónico local válido de 8 dígitos sin caracteres.")]
        [StringLength(20)]
        public string Telefono { get; set; } = string.Empty;
    }
}
