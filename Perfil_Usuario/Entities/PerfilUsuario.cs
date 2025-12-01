using System.Globalization;

namespace Perfil_Usuario.Entities
{
    public class PerfilUsuario
    {
        public string Email { get; set; } = string.Empty;
        public string Tipo_Identificacion { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;

        public string Contraseña { get; set; } = string.Empty;

    }
}
