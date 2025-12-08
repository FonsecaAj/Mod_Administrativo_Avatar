using ApiMAT2.Entities;
using System.Collections.Generic;

namespace ApiMAT2.Repository
{
    public interface IMatriculaRepository
    {
        void Crear(Matricula matricula);
        void Actualizar(Matricula matricula);
        void Eliminar(int id);

        int? ObtenerIdEstudiantePorIdentificacion(string identificacion);
        bool PeriodoEsActivo(int idPeriodo);
        bool GrupoPerteneceACurso(int idGrupo, int idCurso);

        IEnumerable<MatriculaListadoDto> ObtenerPorCursoYGrupo(int idCurso, int idGrupo);

    }
}
