namespace Avatar_Mod_Administración.Entities
{
    public class RubroDto
    {

        public int ID_Rubro { get; set; }
        public int ID_Grupo { get; set; }
        public string Nombre_Rubro { get; set; } = string.Empty;
        public int Porcentaje { get; set; }

    }

    public class DesgloseRequest
    {
        public int ID_Grupo { get; set; }
        public List<RubroDto> Rubros { get; set; } = new();
    }

}
