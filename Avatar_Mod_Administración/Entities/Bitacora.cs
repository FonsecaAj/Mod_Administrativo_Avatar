namespace Avatar_Mod_Administración.Entities
{
    public class Bitacora
    {
        public int ID_Bitacora { get; set; }
        public DateTime Fecha_Registro { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? Tipo_Accion { get; set; }
    }

    public class BitacoraCrearDto
    {
        public string Usuario { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? Tipo_Accion { get; set; }
    }

    public class BitacoraFiltroDto
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? Usuario { get; set; }
        public string? Accion { get; set; }
        public string? Modulo { get; set; }
    }
}