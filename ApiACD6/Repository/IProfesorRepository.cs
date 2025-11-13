
using System.Collections.Generic;
using ApiACD6.Entities;

namespace ApiACD6.Repository
{
    public interface IProfesorRepository
    {
        IEnumerable<Profesor> ObtenerTodos();
        Profesor ObtenerPorId(int idProfesor);
        void Crear(Profesor profesor);
        void Actualizar(Profesor profesor);
        void Eliminar(int idProfesor);
    }
}
