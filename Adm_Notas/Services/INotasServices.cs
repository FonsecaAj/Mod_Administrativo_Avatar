namespace Adm_Notas.Entities
{
    public interface INotasServices
    {

        //// =========================================================
        //// NOTAS POR RUBRO
        //// =========================================================

        Task<BusinessLogicResponse> AsignarNota(NotaRequest request);


        Task<BusinessLogicResponse> ObtenerNotas(int idEstudiante, int idCurso);

    }


}
