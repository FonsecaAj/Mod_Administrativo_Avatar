namespace Avatar_Mod_Administración.Entities
{
    public class Bitacora
    {
        public int iD_Bitacora { get; set; }
        public DateTime fecha_Registro { get; set; }
        public string usuario { get; set; } = "";
        public string tipo_Accion { get; set; } = "";
        public object detalle { get; set; } = new();
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
        public int Pagina { get; set; } = 1;
        public int PorPagina { get; set; } = 10;
    }
}