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

        // Propiedad de navegación necesaria para Dapper Multi-mapping en Facturas_UsuarioAsync
        public Estudiante? Estudiante { get; set; }
    }

    // --- Entidad Estudiante (Para el Join en Facturas_UsuarioAsync) ---
    public class Estudiante
    {
        public int ID_Estudiante { get; set; }
        public string Identificacion { get; set; } = string.Empty;

        // Esta propiedad es crucial para mapear 'Nombre_Completo' de la consulta SQL.
        public string Nombre_Completo { get; set; } = string.Empty;

        // También incluimos las partes individuales del nombre por si son necesarias en otras consultas
        public string Nombre { get; set; } = string.Empty;
        public string Apellido1 { get; set; } = string.Empty;
        public string Apellido2 { get; set; } = string.Empty;
    }


}


