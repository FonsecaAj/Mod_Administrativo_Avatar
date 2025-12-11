using Microsoft.AspNetCore.Mvc;
using Avatar_Mod_Administración.Services;

namespace Avatar_Mod_Administración.Pages
{
    public class IndexModel : BasePageModel
    {
        private readonly IInstitucionService _institucionService;

        // Propiedades públicas para la vista
        public string UsuarioEmailPublic => UsuarioEmail;
        public string UsuarioNombrePublic => UsuarioNombre;
        public string UsuarioRolPublic => UsuarioRol;
        public int UsuarioRolIdPublic => UsuarioRolId;

        public int TotalUsuarios { get; set; } = 0;
        public int TotalInstituciones { get; set; } = 0;
        public int TotalCarreras { get; set; } = 0;
        public int TotalCursos { get; set; } = 0;

        public IndexModel(
            IAuthService authService,
            IUsuarioService usuarioService,
            IInstitucionService institucionService,
            ILogger<IndexModel> logger)
            : base(authService, usuarioService, logger)
        {
            _institucionService = institucionService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var redirectResult = await InicializarSesionAsync();
                if (redirectResult != null)
                {
                    return redirectResult; // Redirigir si no hay sesión válida
                }

                _logger.LogInformation("Dashboard cargado para: {Email}", UsuarioEmail);

                // Cargar estadísticas según el rol
                var token = ObtenerToken();
                if (!string.IsNullOrEmpty(token))
                {
                    await CargarEstadisticasAsync(token);
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el dashboard");
                TempData["Error"] = "Ocurrió un error al cargar el dashboard";
                return Page();
            }
        }

        private async Task CargarEstadisticasAsync(string token)
        {
            try
            {
                // Obtener total de usuarios
                var usuarios = await _usuarioService.ObtenerTodosAsync(token);
                TotalUsuarios = usuarios?.Count ?? 0;

                // Obtener total de instituciones
                var instituciones = await _institucionService.ObtenerTodosAsync(token);
                TotalInstituciones = instituciones?.Count ?? 0;

                // Cuando implementes los servicios de Carreras y Cursos, añádelos aquí
                TotalCarreras = 0;
                TotalCursos = 0;

                _logger.LogDebug("Estadisticas: {Usuarios} usuarios, {Instituciones} instituciones",
                    TotalUsuarios, TotalInstituciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar estadísticas");
            }
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            return await CerrarSesionAsync();
        }
    }
}