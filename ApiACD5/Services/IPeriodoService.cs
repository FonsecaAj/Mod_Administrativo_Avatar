using ApiACD5.Entities;

public interface IPeriodoService
{
    Task<BusinessLogicResponse> ObtenerTodos();
    Task<BusinessLogicResponse> ObtenerPorId(int id);
    Task<BusinessLogicResponse> Crear(Periodo periodo);
    Task<BusinessLogicResponse> Modificar(Periodo periodo);
    Task<BusinessLogicResponse> Eliminar(int id);
}
