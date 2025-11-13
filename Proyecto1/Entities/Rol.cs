namespace USR2.Entities
{
    public class Rol
    {
        public int IdRol { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }

    public class RolModulo
    {
        public int IdRolModulo { get; set; }
        public int IdRol { get; set; }
        public int IdModulo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }

    // DTO para obtener info de módulos desde USR4
    public class ModuloDto
    {
        public int IdModulo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    // DTO para respuesta enriquecida
    public class RolModuloDetalleDto
    {
        public int IdRolModulo { get; set; }
        public int IdRol { get; set; }
        public int IdModulo { get; set; }
        public string NombreModulo { get; set; } = string.Empty;
        public bool ModuloActivo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }
}