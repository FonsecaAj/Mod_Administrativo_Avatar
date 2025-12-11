namespace Adm_Notificaciones.Entities
{
    public class NotificacionRequest
    {

        public string Email { get; set; } = string.Empty;
        public string Asunto { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;

    }
}
