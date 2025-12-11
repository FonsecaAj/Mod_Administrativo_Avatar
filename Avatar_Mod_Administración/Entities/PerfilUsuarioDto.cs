namespace Avatar_Mod_Administración.Entities
{
    public class PerfilUsuarioDto
    {

        public string Email { get; set; } = "";
        public string Tipo_Identificacion { get; set; } = "";
        public string Identificacion { get; set; } = ""; 
        public string Nombre { get; set; } = "";
    }


    public class CambiarContrasenaRequest
    {
        public string Email { get; set; } = "";
        public string ContrasenaNueva { get; set; } = "";
    }
}
