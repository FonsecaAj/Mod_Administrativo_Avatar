using System.ComponentModel.DataAnnotations;

namespace ApiMAT2.Entities
{
    public class MatriculaRequest
    {
        [Required(ErrorMessage = "La identificación del estudiante es requerida.")]
        public string Identificacion { get; set; }

        [Required(ErrorMessage = "El curso es requerido.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un ID de curso válido.")]
        public int ID_Curso { get; set; }

        [Required(ErrorMessage = "El grupo es requerido.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un ID de grupo válido.")]
        public int ID_Grupo { get; set; }

        [Required(ErrorMessage = "El periodo es requerido.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un ID de periodo válido.")]
        public int ID_Periodo { get; set; }
    }
}
