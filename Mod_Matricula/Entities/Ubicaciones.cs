namespace Mod_Matricula.Entities
{
    public class Provincia
    {
        public int ID_Provincia { get; set; }
        public string Nombre_Provincia { get; set; } = string.Empty;
    }

    public class Canton
    {
        public int ID_Canton { get; set; }
        public int ID_Provincia { get; set; }
        public string Nombre_Canton { get; set; } = string.Empty;
    }

    public class Distrito
    {
        public int ID_Distrito { get; set; }
        public int ID_Provincia { get; set; }
        public int ID_Canton { get; set; }
        public string Nombre_Distrito { get; set; } = string.Empty;
    }
}
