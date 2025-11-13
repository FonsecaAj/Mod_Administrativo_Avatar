namespace ADM_Pagos.Entities
{
    public class FacturaDatos
    {
        public int ID_Factura { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = "Pendiente";
    }

    public class EnvelopeResponse<T>
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public T? ResponseObject { get; set; }
    }
}
