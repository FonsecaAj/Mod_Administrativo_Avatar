using ApiMAT1.Entities;


namespace ApiMAT1.Repository
{
    public interface IPrematriculaRepository
    {
        IEnumerable<dynamic> ObtenerTodas();  
        Prematricula? ObtenerPorId(int id);
        void Crear(Prematricula entidad);
        void Actualizar(Prematricula entidad);
        void Eliminar(int id);
    }
}
