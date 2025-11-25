namespace Avatar_Mod_Administración.Entities
{
    public class NotificacionRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Asunto { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;

    }

    public class BusinessLogicResponseNotificacion
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? ResponseObject { get; set; }
    }
}
