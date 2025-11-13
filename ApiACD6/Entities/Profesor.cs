namespace ApiACD6.Entities
{
    public class Profesor
    {
        public int ID_Profesor { get; set; }
        public string Numero_Identificacion { get; set; } = string.Empty;
        public string Tipo_Identificacion { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Nombre_Completo { get; set; } = string.Empty;
        public DateTime Fecha_Nacimiento { get; set; }
        public string Telefono { get; set; } = string.Empty;


    }
}
