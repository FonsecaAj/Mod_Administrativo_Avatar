
namespace Avatar_Mod_Administración.Entities
{
    public class ProvinciaDto
    {
        public int ID_Provincia { get; set; }
        public string Nombre_Provincia { get; set; } = string.Empty;
    }

    public class CantonDto
    {
        public int ID_Canton { get; set; }
        public int ID_Provincia { get; set; }
        public string Nombre_Canton { get; set; } = string.Empty;
    }

    public class DistritoDto
    {
        public int ID_Distrito { get; set; }
        public int ID_Provincia { get; set; }
        public int ID_Canton { get; set; }
        public string Nombre_Distrito { get; set; } = string.Empty;
    }
}
