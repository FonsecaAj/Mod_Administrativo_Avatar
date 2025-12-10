using System.ComponentModel.DataAnnotations;

namespace Avatar_Mod_Administración.Entities
{
    public class ExpedienteRequestDto
    {
        [Display(Name = "Número de identificación")]
        [Required(ErrorMessage = "La identificación es obligatoria.")]
        public string Numero_Identificacion { get; set; } = string.Empty;

        [Display(Name = "Tipo de identificación")]
        [Required(ErrorMessage = "El tipo de identificación es obligatorio.")]
        public string Tipo_Identificacion { get; set; } = string.Empty;

        [Display(Name = "Correo electrónico")]
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo no válido.")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Nombre completo")]
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        public string Nombre_Completo { get; set; } = string.Empty;

        [Display(Name = "Fecha de nacimiento")]
        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime Fecha_Nacimiento { get; set; }

        [Display(Name = "Provincia")]
        [Required(ErrorMessage = "Debe seleccionar una provincia.")]
        public int ID_Provincia { get; set; }

        [Display(Name = "Cantón")]
        [Required(ErrorMessage = "Debe seleccionar un cantón.")]
        public int ID_Canton { get; set; }

        [Display(Name = "Distrito")]
        [Required(ErrorMessage = "Debe seleccionar un distrito.")]
        public int DistritoID { get; set; }

        [Display(Name = "Teléfonos")]
        [Required(ErrorMessage = "Debe ingresar al menos un teléfono.")]
        public string Telefonos { get; set; } = string.Empty;
    }
}
