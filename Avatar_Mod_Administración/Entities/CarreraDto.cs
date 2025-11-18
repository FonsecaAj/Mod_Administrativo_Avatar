namespace Avatar_Mod_Administración.Entities
{
    public class CarreraDto
    {

        public int ID_Carrera { get; set; }
        public string Identificador { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Institucion { get; set; } = string.Empty;
        public int ID_Profesor { get; set; } 
        public string Nombre_Director { get; set; } = string.Empty;
        public int Estado { get; set; }

    }
}
