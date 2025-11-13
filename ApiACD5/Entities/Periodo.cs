using System;
using System.ComponentModel.DataAnnotations;

namespace ApiACD5.Entities
{
    public class Periodo
    {
        [Key]
        public int ID_Periodo { get; set; }

        [Required(ErrorMessage = "El año es requerido.")]
        public int Año { get; set; }

        [Required(ErrorMessage = "El número de periodo es requerido.")]
        public int Numero_Periodo { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es requerida.")]
        public DateTime Fecha_Inicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es requerida.")]
        public DateTime Fecha_Fin { get; set; }

        [Required]
        public string Estado { get; set; } = "Futuro"; 


    }
}
