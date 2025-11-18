namespace Avatar_Mod_Administración.Entities
{
    public class FacturaDetalleDto
    {
        public int ID_Detalle { get; set; }
        public int ID_Factura { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal Monto { get; set; }

        public string? Detalle { get; set; }
    }
}
