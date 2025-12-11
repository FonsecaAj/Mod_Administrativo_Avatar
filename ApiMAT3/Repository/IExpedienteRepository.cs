using ApiMAT3.Entities;
using System.Collections.Generic;

namespace ApiMAT3.Repository
{
    public interface IExpedienteRepository
    {
        IEnumerable<Expediente> ObtenerTodos();
        Expediente ObtenerPorId(string numeroIdentificacion);
        void Crear(Expediente expediente);
        void Actualizar(Expediente expediente);
        void Eliminar(string numeroIdentificacion);
    }
}
