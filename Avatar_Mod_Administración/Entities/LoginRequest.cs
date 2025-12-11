using System.ComponentModel.DataAnnotations;

namespace Avatar_Mod_Administración.Entities
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@(cuc\.cr|cuc\.ac\.cr)$",
            ErrorMessage = "Solo se permiten dominios @cuc.cr o @cuc.ac.cr")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Contrasenna { get; set; } = string.Empty;

        public bool RecordarSesion { get; set; }
    }

    public class LoginResponse
    {
        public DateTime ExpiresIn { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string UsuarioID { get; set; } = string.Empty;
    }

    public class RefreshRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshResponse
    {
        public DateTime ExpiresIn { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class ValidateRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}