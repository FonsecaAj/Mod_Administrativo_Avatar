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
        private readonly IBitacoraService _bitacoraService;

        public string UsuarioEmailPublic => UsuarioEmail;
        public string UsuarioNombrePublic => UsuarioNombre;
        public string UsuarioRolPublic => UsuarioRol;
        public int UsuarioRolIdPublic => UsuarioRolId;

        public int TotalUsuarios { get; set; } = 0;
        public int TotalInstituciones { get; set; } = 0;
        public int TotalCarreras { get; set; } = 0;
        public int TotalCursos { get; set; } = 0;
        public int TotalPeriodosActivos { get; set; } = 0;
        public int TotalFacturasPendientes { get; set; } = 0;
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
            IBitacoraService bitacoraService,
            ILogger<IndexModel> logger)
            : base(authService, usuarioService, logger)
        {
            _institucionService = institucionService;
            _cursoApiClient = cursoApiClient;
            _periodoApiClient = periodoApiClient;
            _grupoApiClient = grupoApiClient;
            _facturaApiClient = facturaApiClient;
            _profesorApiClient = profesorApiClient;
            _bitacoraService = bitacoraService;
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
                    // Registrar visualización del dashboard en bitácora
                    await RegistrarVisualizacionDashboardAsync(token);

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

        private async Task RegistrarVisualizacionDashboardAsync(string token)
        {
            try
            {
                var bitacoraDto = new BitacoraCrearDto
                {
                    Usuario = UsuarioEmail,
                    Tipo_Accion = "SELECT",
                    Descripcion = $"Visualización del dashboard por {UsuarioNombre} ({UsuarioRol})"
                };

                var registrado = await _bitacoraService.RegistrarAsync(bitacoraDto, token);

                if (registrado)
                {
                    _logger.LogDebug("Visualización del dashboard registrada en bitácora para {Email}", UsuarioEmail);
                }
                else
                {
                    _logger.LogWarning("No se pudo registrar la visualización del dashboard en bitácora para {Email}", UsuarioEmail);
                }
            }
            catch (Exception ex)
            {
                // No fallar la carga del dashboard si el registro en bitácora falla
                _logger.LogWarning(ex, "Error al registrar visualización del dashboard en bitácora para {Email}", UsuarioEmail);
            }
        }

        // Paralelizar las llamadas en lugar de secuenciales
        private async Task CargarEstadisticasAsync(string token)
        {
            try
            {
                if (UsuarioRol.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
                {
                    // Cargar todas las estadísticas en paralelo
                    await Task.WhenAll(
                        CargarUsuariosAsync(token),
                        CargarInstitucionesAsync(token),
                        CargarCursosYCarrerasAsync(token),
                        CargarPeriodosActivosAsync(token),
                        CargarFacturasPendientesAsync(token)
                    );
                }
                else if (UsuarioRol.Equals("Profesor", StringComparison.OrdinalIgnoreCase))
                {
                    // Profesor: estadísticas generales + específicas en paralelo
                    await Task.WhenAll(
                        CargarUsuariosAsync(token),
                        CargarInstitucionesAsync(token),
                        CargarCursosYCarrerasAsync(token),
                        CargarEstadisticasProfesorAsync(token)
                    );
                }
                else
                {
                    // Otros roles: solo estadísticas generales en paralelo
                    await Task.WhenAll(
                        CargarUsuariosAsync(token),
                        CargarInstitucionesAsync(token),
                        CargarCursosYCarrerasAsync(token)
                    );
                }

                _logger.LogDebug("Estadísticas cargadas correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar estadísticas");
            }
        }

        private async Task CargarUsuariosAsync(string token)
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
        }

        private async Task CargarInstitucionesAsync(string token)
        {
            try
            {
                var instituciones = await _institucionService.ObtenerTodosAsync(token);
                TotalInstituciones = instituciones?.Count ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener total de instituciones");
            }
        }

        private async Task CargarCursosYCarrerasAsync(string token)
        {
            try
            {
                // Paralelizar las 2 llamadas de cursos
                var cursosTask = _cursoApiClient.ObtenerTodosAsync();
                var lookupsTask = _cursoApiClient.ObtenerLookupsAsync();

                await Task.WhenAll(cursosTask, lookupsTask);

                TotalCursos = cursosTask.Result?.Count() ?? 0;
                TotalCarreras = lookupsTask.Result?.Carreras?.Count() ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener cursos y carreras");
            }
        }

        private async Task CargarPeriodosActivosAsync(string token)
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
        }

        private async Task CargarFacturasPendientesAsync(string token)
        {
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
                // Paralelizar obtención de profesores, grupos y periodos
                var profesoresTask = _profesorApiClient.ObtenerTodosAsync();
                var gruposTask = _grupoApiClient.ObtenerTodosAsync();
                var periodosTask = _periodoApiClient.ObtenerTodosAsync();

                await Task.WhenAll(profesoresTask, gruposTask, periodosTask);

                var profesores = profesoresTask.Result;
                var grupos = gruposTask.Result;
                var periodos = periodosTask.Result;

                // Procesar datos del profesor
                var profesorActual = profesores?.FirstOrDefault(p =>
                    !string.IsNullOrWhiteSpace(p.Email) &&
                    p.Email.Equals(UsuarioEmail, StringComparison.OrdinalIgnoreCase)
                );

                if (profesorActual != null)
                {
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

                // Procesar periodo actual
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
                _logger.LogError(ex, "Error al obtener estadísticas del profesor");
            }
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            return await CerrarSesionAsync();
        }
    }
}