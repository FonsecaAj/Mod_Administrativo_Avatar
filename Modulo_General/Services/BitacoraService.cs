using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Repository;
using System.Text.Json;

namespace Avatar_Mod_Administración.Services
{
    public class BitacoraService : IBitacoraService
    {
        private readonly BitacoraRepository _bitacoraRepository;

        public BitacoraService(BitacoraRepository bitacoraRepository)
        {
            _bitacoraRepository = bitacoraRepository;
        }

        public async Task<BusinessLogicResponse> Registrar(BitacoraRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Usuario))
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "El campo 'Usuario' es requerido y no puede estar vacío."
                    };

                if (string.IsNullOrWhiteSpace(request.Descripcion))
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "El campo 'Descripción' es requerida y no puede estar vacía."
                    };

                var tipoAccion = string.IsNullOrWhiteSpace(request.Tipo_Accion)
                    ? "INSERT"
                    : request.Tipo_Accion.Trim().ToUpper();

                if (tipoAccion != "SELECT" && tipoAccion != "GENERICA" && !IsValidJson(request.Descripcion))
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "El campo 'Descripción' debe tener un formato JSON válido para operaciones de tipo INSERT, UPDATE o DELETE."
                    };

                var bitacora = new Bitacora
                {
                    Usuario = request.Usuario.Trim(),
                    Tipo_Accion = tipoAccion,
                    Descripcion = request.Descripcion.Trim(),
                    Fecha_Registro = DateTime.Now
                };

                var id = await _bitacoraRepository.Registrar(bitacora);

                return new BusinessLogicResponse
                {
                    StatusCode = 201,
                    Message = "Bitácora registrada exitosamente.",
                    ResponseObject = new
                    {
                        ID_Bitacora = id,
                        bitacora.Usuario,
                        bitacora.Tipo_Accion,
                        bitacora.Fecha_Registro
                    }
                };
            }
            catch (Exception ex)
            {
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = $"Error al registrar bitácora: {ex.Message}"
                };
            }
        }

        public async Task<BusinessLogicResponse> Consultar(BitacoraFiltroRequest request)
        {
            try
            {
                (IEnumerable<Bitacora>, int) tuple = await _bitacoraRepository.Consultar(request);
                IEnumerable<Bitacora> data = tuple.Item1;
                int total = tuple.Item2;
                var dataFormateada = data.Select((Bitacora x) => new
                {
                    ID_Bitacora = x.ID_Bitacora,
                    Fecha_Registro = x.Fecha_Registro,
                    Usuario = x.Usuario,
                    Tipo_Accion = x.Tipo_Accion,
                    Detalle = ParseDescripcion(x.Descripcion)
                });
                var respuesta = new
                {
                    TotalRegistros = total,
                    Pagina = request.Pagina,
                    PorPagina = request.PorPagina,
                    Data = dataFormateada
                };
                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Registros obtenidos correctamente.",
                    ResponseObject = respuesta

        public async Task<BusinessLogicResponse> ObtenerTodas(string? usuario = null)
        {
            try
            {
                var bitacoras = await _bitacoraRepository.ObtenerTodas(usuario);

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Bitácoras obtenidas exitosamente.",
                    ResponseObject = bitacoras
                };
            }
            catch (Exception ex)
            {
                return new BusinessLogicResponse
                {
                    StatusCode = 500,

                    Message = "Error al consultar bitácora: " + ex.Message

                    Message = $"Error al obtener bitácoras: {ex.Message}"

                };
            }
        }


        // Método auxiliar para validar si la descripción es JSON válido
        private bool IsValidJson(string str)
        {
            try
            {
                JsonDocument.Parse(str);
                return true;
            }
            catch
            {
                return false;
            }
        }


        private object? ParseDescripcion(string descripcionRaw)
        {
            if (string.IsNullOrWhiteSpace(descripcionRaw))
            {
                return null;
            }
            try
            {
                return JsonSerializer.Deserialize<object>(descripcionRaw);
            }
            catch
            {
                return descripcionRaw;
            }
        }

    }
}