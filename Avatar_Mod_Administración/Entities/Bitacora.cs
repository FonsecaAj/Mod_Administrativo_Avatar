namespace Avatar_Mod_Administración.Entities
{
    public class Bitacora
    {
        public int IdBitacora { get; set; }
        public DateTime FechaBitacora { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class BitacoraCrearDto
    {
        public string Usuario { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
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