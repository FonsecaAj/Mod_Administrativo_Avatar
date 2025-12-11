namespace ACD1.Services
{
    public interface ICarreraService
    {
        Task<int> ContarCarrerasPorInstitucionAsync(int idInstitucion, string? authorization);
    }
}
