using ApiMAT2.Entities;
using System.Collections.Generic;

namespace ApiMAT2.Services
{
    public interface IMatriculaService
    {
        void Crear(MatriculaRequest request);
        void Actualizar(MatriculaRequest request);
        void Eliminar(int id);
        IEnumerable<object> ObtenerPorCursoYGrupo(int idCurso, int idGrupo);

        MatriculaLookupsDto ObtenerLookups();

        Task<BusinessLogicResponse> Obtener_Matricula_Estudiante(string identificacion);


    }
}
