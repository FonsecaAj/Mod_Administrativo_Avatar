namespace ApiMAT3.Entities
{
    public class Expediente
    {
        public string Numero_Identificacion { get; set; }
        public string Tipo_Identificacion { get; set; }
        public string Email { get; set; }
        public string Nombre_Completo { get; set; }
        public DateTime Fecha_Nacimiento { get; set; }
        public int ID_Provincia { get; set; }
        public int ID_Canton { get; set; }
        public int DistritoID { get; set; }
        public string Telefonos { get; set; }
    }
}
