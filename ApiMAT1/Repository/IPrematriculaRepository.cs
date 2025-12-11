using ApiMAT1.Entities;
using System.Collections.Generic;

namespace ApiMAT1.Repository
{
    public interface IPrematriculaRepository
    {
        IEnumerable<Prematricula> ObtenerTodas();
        Prematricula? ObtenerPorId(int id);
        void Crear(Prematricula entidad);
        void Actualizar(Prematricula entidad);
        void Eliminar(int id);

        // Nuevo método Jocs
        Task<IEnumerable<PrematriculaDetallada>> Obtener_Prematri_Estudiante(string identificacion);
    }
}
