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

        IEnumerable<PeriodoMatriculaDto> ObtenerPeriodos();
        IEnumerable<CursoMatriculaDto> ObtenerCursos();
        IEnumerable<GrupoMatriculaDto> ObtenerGrupos();

        bool ExisteMatricula(int idEstudiante, int idCurso, int idGrupo);
        int ObtenerCupoDisponible(int idGrupo);


        //OBTENER MATRICULA DE ESTUDIANTE
        Task<IEnumerable<MatriculaEstudianteDto>> Obtener_Matricula_Estudiante(string identificacion);

    }
}
