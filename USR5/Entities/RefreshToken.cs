namespace USR5.Entities
{
    public class RefreshToken
    {
        public int IdRefreshToken { get; set; }
        public string Token { get; set; } = string.Empty;
        public string UsuarioEmail { get; set; } = string.Empty;
        public DateTime FechaExpiracion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Activo { get; set; }
    }
}
