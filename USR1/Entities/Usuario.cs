namespace USR1.Entities
{
    public class Usuario
    {
        public string Email { get; set; } = string.Empty;
        public int IdTipoIdentificacion { get; set; }
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int IdRol { get; set; }
        public string Contrasenna { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public bool Activo { get; set; }
    }
}
