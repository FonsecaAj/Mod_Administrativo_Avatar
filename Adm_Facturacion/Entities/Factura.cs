namespace Adm_Facturacion.Entities
{
    public class Factura
    {
        public int ID_Factura { get; set; }
        public string Identificacion_Estudiante { get; set; } = string.Empty;
        public DateTime Fecha_Emision { get; set; }
        public decimal Monto_Base { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Anulada, Pagada

        public List<FacturaDetalle> Detalles { get; set; } = new();
    }


}


