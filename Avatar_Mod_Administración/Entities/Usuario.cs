namespace Avatar_Mod_Administración.Entities
{
    public class Usuario
    {
        public string Email { get; set; } = string.Empty;
        public int IdTipoIdentificacion { get; set; }
        public string TipoIdentificacion { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int IdRol { get; set; }
        public string RolNombre { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public bool Activo { get; set; }
    }

    public class UsuarioCrearDto
    {
        public string Email { get; set; } = string.Empty;
        public int IdTipoIdentificacion { get; set; }
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Contrasenna { get; set; } = string.Empty;
        public string? RolDeseado { get; set; }
    }

    public class TipoIdentificacion
    {
        public int IdTipoIdentificacion { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class Rol
    {
        public int IdRol { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}