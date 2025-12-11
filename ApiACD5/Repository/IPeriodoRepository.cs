using ApiACD5.Entities;
using System.Collections.Generic;

namespace ApiACD5.Repository
{
    public interface IPeriodoRepository
    {
        IEnumerable<Periodo> ObtenerTodos();
        Periodo? ObtenerPorId(int id);
        int Crear(Periodo periodo);
        int Modificar(Periodo periodo);
        int Eliminar(int id);
    }
}
