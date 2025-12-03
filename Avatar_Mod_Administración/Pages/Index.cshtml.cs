using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;

namespace Avatar_Mod_Administración.Pages
{
    public class IndexModel : BasePageModel
    {
        private readonly IInstitucionService _institucionService;
        private readonly ICursoApiClient _cursoApiClient;
        private readonly IPeriodoApiClient _periodoApiClient;
        private readonly IGrupoApiClient _grupoApiClient;
        private readonly IFacturaApiClient _facturaApiClient;
        private readonly IProfesorApiClient _profesorApiClient;

        // Propiedades públicas para la vista
        public string UsuarioEmailPublic => UsuarioEmail;
        public string UsuarioNombrePublic => UsuarioNombre;
        public string UsuarioRolPublic => UsuarioRol;
        public int UsuarioRolIdPublic => UsuarioRolId;

        // Estadísticas generales
        public int TotalUsuarios { get; set; } = 0;
        public int TotalInstituciones { get; set; } = 0;
        public int TotalCarreras { get; set; } = 0;
        public int TotalCursos { get; set; } = 0;

        // Estadísticas específicas para Administrador
        public int TotalPeriodosActivos { get; set; } = 0;
        public int TotalFacturasPendientes { get; set; } = 0;

        // Estadísticas específicas para Profesor
        public int TotalGruposAsignados { get; set; } = 0;
        public int TotalCursosImpartidos { get; set; } = 0;
        public string PeriodoActual { get; set; } = string.Empty;
        public DateTime? PeriodoFechaInicio { get; set; }
        public DateTime? PeriodoFechaFin { get; set; }
        public int PorcentajePeriodo { get; set; } = 0;

        public IndexModel(
            IAuthService authService,
            IUsuarioService usuarioService,
            IInstitucionService institucionService,
            ICursoApiClient cursoApiClient,
            IPeriodoApiClient periodoApiClient,
            IGrupoApiClient grupoApiClient,
            IFacturaApiClient facturaApiClient,
            IProfesorApiClient profesorApiClient,
            ILogger<IndexModel> logger)
            : base(authService, usuarioService, logger)
        {
            _institucionService = institucionService;
            _cursoApiClient = cursoApiClient;
            _periodoApiClient = periodoApiClient;
            _grupoApiClient = grupoApiClient;
            _facturaApiClient = facturaApiClient;
            _profesorApiClient = profesorApiClient;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var redirectResult = await InicializarSesionAsync();
                if (redirectResult != null)
                {
                    return redirectResult;
                }

                _logger.LogInformation("Dashboard cargado para: {Email} con rol: {Rol}", UsuarioEmail, UsuarioRol);

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
                // Estadísticas comunes a todos los roles
                await CargarEstadisticasGeneralesAsync(token);

                // Estadísticas según el rol
                if (UsuarioRol.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
                {
                    await CargarEstadisticasAdministradorAsync(token);
                }
                else if (UsuarioRol.Equals("Profesor", StringComparison.OrdinalIgnoreCase))
                {
                    await CargarEstadisticasProfesorAsync(token);
                }

                _logger.LogDebug("Estadísticas cargadas correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar estadísticas");
            }
        }

        private async Task CargarEstadisticasGeneralesAsync(string token)
        {
            try
            {
                var usuarios = await _usuarioService.ObtenerTodosAsync(token);
                TotalUsuarios = usuarios?.Count ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener total de usuarios");
            }

            try
            {
                var instituciones = await _institucionService.ObtenerTodosAsync(token);
                TotalInstituciones = instituciones?.Count ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener total de instituciones");
            }

            try
            {
                var cursos = await _cursoApiClient.ObtenerTodosAsync();
                TotalCursos = cursos?.Count() ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener total de cursos");
            }

            try
            {
                var lookups = await _cursoApiClient.ObtenerLookupsAsync();
                TotalCarreras = lookups?.Carreras?.Count() ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener total de carreras");
            }
        }

        private async Task CargarEstadisticasAdministradorAsync(string token)
        {
            try
            {
                var periodos = await _periodoApiClient.ObtenerTodosAsync();
                TotalPeriodosActivos = periodos?.Count(p =>
                    p.Estado.Equals("Activo", StringComparison.OrdinalIgnoreCase)) ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener períodos activos");
            }

            try
            {
                var fechaFin = DateTime.Today;
                var fechaInicio = fechaFin.AddMonths(-3);

                var resultado = await _facturaApiClient.ListarFacturasAsync(
                    fechaInicio,
                    fechaFin,
                    "Pendiente",
                    token
                );

                TotalFacturasPendientes = resultado.ok && resultado.facturas != null
                    ? resultado.facturas.Count
                    : 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener facturas pendientes");
                TotalFacturasPendientes = 0;
            }
        }

        private async Task CargarEstadisticasProfesorAsync(string token)
        {
            try
            {
                var profesores = await _profesorApiClient.ObtenerTodosAsync();

                var profesorActual = profesores?.FirstOrDefault(p =>
                    !string.IsNullOrWhiteSpace(p.Email) &&
                    p.Email.Equals(UsuarioEmail, StringComparison.OrdinalIgnoreCase)
                );

                if (profesorActual != null)
                {
                    var grupos = await _grupoApiClient.ObtenerTodosAsync();

                    var gruposProfesor = grupos?
                        .Where(g => g.IdProfesor == profesorActual.IdProfesor)
                        .ToList() ?? new List<GrupoDto>();

                    TotalGruposAsignados = gruposProfesor.Count;

                    TotalCursosImpartidos = gruposProfesor
                        .Select(g => g.IdCurso)
                        .Distinct()
                        .Count();

                    _logger.LogDebug("Profesor {Email}: {Grupos} grupos, {Cursos} cursos",
                        UsuarioEmail, TotalGruposAsignados, TotalCursosImpartidos);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas del profesor");
            }

            await ObtenerPeriodoActualAsync();
        }

        private async Task ObtenerPeriodoActualAsync()
        {
            try
            {
                var periodos = await _periodoApiClient.ObtenerTodosAsync();

                var periodoActivo = periodos?.FirstOrDefault(p =>
                    p.Estado.Equals("Activo", StringComparison.OrdinalIgnoreCase));

                if (periodoActivo != null)
                {
                    PeriodoActual = $"{periodoActivo.NumeroPeriodo} Cuatrimestre {periodoActivo.Anno}";
                    PeriodoFechaInicio = periodoActivo.FechaInicio;
                    PeriodoFechaFin = periodoActivo.FechaFin;

                    var hoy = DateTime.Today;
                    if (hoy >= periodoActivo.FechaInicio && hoy <= periodoActivo.FechaFin)
                    {
                        var diasTotales = (periodoActivo.FechaFin - periodoActivo.FechaInicio).TotalDays;
                        var diasTranscurridos = (hoy - periodoActivo.FechaInicio).TotalDays;

                        if (diasTotales > 0)
                        {
                            PorcentajePeriodo = (int)Math.Round((diasTranscurridos / diasTotales) * 100);
                            PorcentajePeriodo = Math.Max(0, Math.Min(100, PorcentajePeriodo));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener período actual");
            }
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            return await CerrarSesionAsync();
        }
    }
}