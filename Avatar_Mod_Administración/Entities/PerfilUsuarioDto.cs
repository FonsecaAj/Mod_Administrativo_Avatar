namespace Avatar_Mod_Administración.Entities
{
    public class PerfilUsuarioDto
    {

        public string Email { get; set; } = "";
        public string Tipo_Identificacion { get; set; } = "";
        public string Identificacion { get; set; } = "";   // ✔ ESTE NOMBRE ES CRÍTICO
        public string Nombre { get; set; } = "";
        public string Rol { get; set; } = ""; // Si no hay rol, dejar vacío


    }
}
