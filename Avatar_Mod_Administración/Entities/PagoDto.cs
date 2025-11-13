namespace Avatar_Mod_Administración.Entities
{
    public class PagoDto
    {
        public int ID_Pago { get; set; }
        public int ID_Factura { get; set; }
        public DateTime Fecha_Pago { get; set; }
        public decimal Monto_Pago { get; set; }
        public string Metodo_Pago { get; set; } = string.Empty;
        public string Estado { get; set; } = "Completado";
    
    }

    public class PagoDetalleDto
    {
        public int ID_Detalle { get; set; }
        public int ID_Pago { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string? Detalle { get; set; } // Puede ser nulo
    }

    public class PagoRequestDto
    {
        public int ID_Factura { get; set; }
        public string Metodo_Pago { get; set; } = "Tarjeta";
    }

    public class PagoConDetallesDto
    {
        public PagoDto Pago { get; set; } = new();
        public IEnumerable<PagoDetalleDto> Detalles { get; set; } = Enumerable.Empty<PagoDetalleDto>();
    }
}
