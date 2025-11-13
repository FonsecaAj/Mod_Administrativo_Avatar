namespace Adm_Facturacion.Entities
{
    public class FacturaDetalle
    {

        public int ID_Detalle { get; set; }
        public int ID_Factura { get; set; }
        public string Descripcion { get; set; } = "Servicios estudiantiles";
        public decimal Monto { get; set; }

    }
}
