namespace Avatar_Mod_Administración.Entities
{
    // Entidad Rol
    public class RolApi
    {
        public int IdRol { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }

    // DTO para crear/actualizar rol
    public class RolCrearDto
    {
        public string Nombre { get; set; } = string.Empty;
    }

    // Entidad para permisos relación rol-módulo con detalles
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

    // DTO para respuesta del menú
    public class MenuDto
    {
        public List<Modulo> Modulos { get; set; } = new();
    }
}