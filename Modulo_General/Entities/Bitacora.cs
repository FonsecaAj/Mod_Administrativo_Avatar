namespace Avatar_Mod_Administración.Entities
{
    public class Bitacora
    {
        public int ID_Bitacora { get; set; }
        public DateTime Fecha_Registro { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? Tipo_Accion { get; set; } // opcional 
    }

    public class BitacoraRequest
    {
        public string Usuario { get; set; } = string.Empty;
        public string? Tipo_Accion { get; set; }// "INSERT", "UPDATE", "DELETE", "SELECT", "ERROR"
        public string Descripcion { get; set; } = string.Empty; // JSON o texto
    }

    public class BusinessLogicResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? ResponseObject { get; set; }
    }
}
