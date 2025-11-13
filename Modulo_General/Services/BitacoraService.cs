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
                // Validaciones requeridas
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

                // "INSERT"
                var tipoAccion = string.IsNullOrWhiteSpace(request.Tipo_Accion)
                    ? "INSERT"
                    : request.Tipo_Accion.Trim().ToUpper();

                // Validar formato JSON solo para operaciones CRUD (no SELECT ni GENERICA)
                if (tipoAccion != "SELECT" && tipoAccion != "GENERICA" && !IsValidJson(request.Descripcion))
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "El campo 'Descripción' debe tener un formato JSON válido para operaciones de tipo INSERT, UPDATE o DELETE."
                    };

                // Construir entidad Bitácora
                var bitacora = new Bitacora
                {
                    Usuario = request.Usuario.Trim(),
                    Tipo_Accion = tipoAccion,
                    Descripcion = request.Descripcion.Trim(),
                    Fecha_Registro = DateTime.Now
                };

                // Registrar bitácora
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
                // Manejo de errores 
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = $"Error al registrar bitácora: {ex.Message}"
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
    }
}
