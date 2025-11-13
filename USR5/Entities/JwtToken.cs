namespace USR5.Entities
{
    public class JwtToken
    {
        public int IdJwtToken { get; set; }
        public string Token { get; set; } = string.Empty;
        public string UsuarioEmail { get; set; } = string.Empty;
        public DateTime FechaExpiracion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Activo { get; set; }
    }
}
