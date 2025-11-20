namespace Avatar_Mod_Administración.Entities
{
    public class FacturaDto
    {
        public int ID_Factura { get; set; }
        public string Identificacion_Estudiante { get; set; } = string.Empty;
        public DateTime Fecha_Emision { get; set; }
        public decimal Monto_Base { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = string.Empty;

        public List<FacturaDetalleDto> Detalles { get; set; } = new();

    }

    public class CrearFacturaRequest
    {
        public string Identificacion { get; set; } = string.Empty;
    }
}
