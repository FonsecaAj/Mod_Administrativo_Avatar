//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Avatar_Mod_Administración.Entities;
//using Avatar_Mod_Administración.Services;
//using System.Text.Json;

//namespace Avatar_Mod_Administración.Pages.Api
//{
//    public class BitacoraModel : PageModel
//    {
//        private readonly IBitacoraService _bitacoraService;
//        private readonly IAuthService _authService;
//        private readonly ILogger<BitacoraModel> _logger;

//        public BitacoraModel(
//            IBitacoraService bitacoraService,
//            IAuthService authService,
//            ILogger<BitacoraModel> logger)
//        {
//            _bitacoraService = bitacoraService;
//            _authService = authService;
//            _logger = logger;
//        }

//        public async Task<IActionResult> OnPostAsync()
//        {
//            try
//            {
//                // Verificar autenticación
//                var sesion = _authService.ObtenerSesionActual();
//                if (sesion == null)
//                {
//                    _logger.LogWarning("Intento de registrar bitácora sin sesión activa");
//                    return Unauthorized(new { error = "No autorizado" });
//                }

//                // Leer el cuerpo de la solicitud
//                using var reader = new StreamReader(Request.Body);
//                var body = await reader.ReadToEndAsync();

//                _logger.LogDebug("Body recibido para bitácora: {Body}", body);

//                var dto = JsonSerializer.Deserialize<BitacoraCrearDto>(body, new JsonSerializerOptions
//                {
//                    PropertyNameCaseInsensitive = true
//                });

//                if (dto == null || string.IsNullOrWhiteSpace(dto.Usuario) || string.IsNullOrWhiteSpace(dto.Descripcion))
//                {
//                    _logger.LogWarning("Datos de bitácora inválidos");
//                    return BadRequest(new { error = "Datos inválidos" });
//                }

//                // Registrar bitácora en el servicio GEN1
//                var resultado = await _bitacoraService.RegistrarAsync(dto, sesion.AccessToken);

//                if (resultado)
//                {
//                    _logger.LogInformation("Bitácora registrada: {Usuario} - {Descripcion}",
//                        dto.Usuario, dto.Descripcion.Substring(0, Math.Min(50, dto.Descripcion.Length)));
//                    return new JsonResult(new { success = true });
//                }
//                else
//                {
//                    _logger.LogError("No se pudo registrar la bitácora");
//                    return StatusCode(500, new { error = "Error al registrar bitácora" });
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error al procesar registro de bitácora");
//                return StatusCode(500, new { error = "Error interno del servidor" });
//            }
//        }

//        private IActionResult Unauthorized(object value)
//        {
//            throw new NotImplementedException();
//        }

//        public async Task<IActionResult> OnGetAsync([FromQuery] string? usuario,
//            [FromQuery] DateTime? fechaInicio,
//            [FromQuery] DateTime? fechaFin)
//        {
//            try
//            {
//                // Verificar autenticación
//                var sesion = _authService.ObtenerSesionActual();
//                if (sesion == null)
//                {
//                    return Unauthorized(new { error = "No autorizado" });
//                }

//                var filtro = new BitacoraFiltroDto
//                {
//                    Usuario = usuario,
//                    FechaInicio = fechaInicio,
//                    FechaFin = fechaFin
//                };

//                var bitacoras = await _bitacoraService.ObtenerTodosAsync(sesion.AccessToken, filtro);
//                return new JsonResult(bitacoras);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error al obtener bitácoras");
//                return StatusCode(500, new { error = "Error interno del servidor" });
//            }
//        }
//    }
//}