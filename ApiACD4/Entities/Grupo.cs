using System.ComponentModel.DataAnnotations;

namespace ApiACD4.Entities
{
    public class Grupo
    {
        [Key]
        public int ID_Grupo { get; set; }

        [Required(ErrorMessage = "El número del grupo es requerido.")]
        public int Numero_Grupo { get; set; }

        [Required(ErrorMessage = "El curso asignado es requerido.")]
        public int ID_Curso { get; set; }

        [Required(ErrorMessage = "El profesor asignado es requerido.")]
        public int ID_Profesor { get; set; }

        [Required(ErrorMessage = "El horario es requerido.")]
        [StringLength(100, ErrorMessage = "El horario no puede superar los 100 caracteres.")]
        public string Horario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El periodo es requerido.")]
        [StringLength(20, ErrorMessage = "El periodo no puede superar los 20 caracteres.")]
        public int ID_Periodo { get; set; }
    }
}
