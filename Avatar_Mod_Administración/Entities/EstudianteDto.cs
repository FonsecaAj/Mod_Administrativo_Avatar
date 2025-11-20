namespace Avatar_Mod_Administración.Entities
{
    public class EstudianteDto
    {

        public int ID_Estudiante { get; set; }
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido1 { get; set; } = string.Empty;
        public string Apellido2 { get; set; } = string.Empty;

        public string NombreCompleto => $"{Nombre} {Apellido1} {Apellido2}";
    }
}
