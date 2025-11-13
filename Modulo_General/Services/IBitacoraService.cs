using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IBitacoraService
    {
        Task<BusinessLogicResponse> Registrar(BitacoraRequest request);

    }
}
