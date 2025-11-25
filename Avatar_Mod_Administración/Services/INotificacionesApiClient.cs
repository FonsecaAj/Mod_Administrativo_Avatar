using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface INotificacionesApiClient
    {

        Task<BusinessLogicResponseNotificacion?> EnviarCorreoAsync(NotificacionRequestDto request, string? token);

    }
}
