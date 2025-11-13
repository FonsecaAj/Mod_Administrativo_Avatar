namespace ADM_Pagos.Entities
{
    public class Pago
    {
        public int ID_Pago { get; set; }
        public int ID_Factura { get; set; }
        public DateTime Fecha_Pago { get; set; }
        public decimal Monto_Pago { get; set; }
        public string Metodo_Pago { get; set; } = string.Empty;
        public string Estado { get; set; } = "Completado";
    }

    public class PagoDetalle
    {
        public int ID_Detalle { get; set; }
        public int ID_Pago { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public class PagoRequest
    {
        public int ID_Factura { get; set; }
        public string Metodo_Pago { get; set; } = "Tarjeta";
    }

    public class PagoConDetalles
    {
        public Pago Pago { get; set; } = new();
        public IEnumerable<PagoDetalle> Detalles { get; set; } = Enumerable.Empty<PagoDetalle>();
    }
}
