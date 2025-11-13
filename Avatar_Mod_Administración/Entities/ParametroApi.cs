namespace Avatar_Mod_Administración.Entities
{
    public class ParametroApi
    {
        public string IdParametro { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }

    public class ParametroCrearDto
    {
        public string IdParametro { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
    }

    public class RespuestaPaginada<T>
    {
        public List<T> Datos { get; set; } = new();
        public PaginacionInfo Paginacion { get; set; } = new();
    }

    public class PaginacionInfo
    {
        public int TotalRegistros { get; set; }
        public int Pagina { get; set; }
        public int PorPagina { get; set; }
        public int TotalPaginas { get; set; }
    }
}