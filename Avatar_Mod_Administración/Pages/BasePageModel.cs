using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Avatar_Mod_Administración.Services;

namespace Avatar_Mod_Administración.Pages
{
    public abstract class BasePageModel : PageModel
    {
        protected readonly IAuthService _authService;
        protected readonly IUsuarioService _usuarioService;
        protected readonly ILogger _logger;

        protected string UsuarioEmail => HttpContext.Session.GetString("UsuarioEmail") ?? "";
        protected string UsuarioNombre => HttpContext.Session.GetString("UsuarioNombre") ?? "";
        protected string UsuarioRol => HttpContext.Session.GetString("UsuarioRol") ?? "";
        protected int UsuarioRolId
        {
            get
            {
                var rolIdStr = HttpContext.Session.GetString("UsuarioRolId");
                return int.TryParse(rolIdStr, out var rolId) ? rolId : 0;
            }
        }

        protected BasePageModel(
            IAuthService authService,
            IUsuarioService usuarioService,
            ILogger logger)
        {
            _authService = authService;
            _usuarioService = usuarioService;
            _logger = logger;
        }

        protected async Task<IActionResult?> InicializarSesionAsync()
        {
            try
            {
                var sesion = _authService.ObtenerSesionActual();
                if (sesion == null)
                {
                    _logger.LogWarning("No hay sesión activa, redirigiendo a Login");
                    return RedirectToPage("/Login");
                }

                var renovado = await _authService.RenovarSesionAutomaticaAsync();
                if (!renovado)
                {
                    _logger.LogWarning("Token expirado y no se pudo renovar");
                    return RedirectToPage("/Login");
                }

                // Obtener datos de sesión HTTP
                var usuarioEmail = HttpContext.Session.GetString("UsuarioEmail");
                var usuarioNombre = HttpContext.Session.GetString("UsuarioNombre");
                var usuarioRol = HttpContext.Session.GetString("UsuarioRol");
                var usuarioRolIdStr = HttpContext.Session.GetString("UsuarioRolId");

                // Si no hay datos en sesión, obtenerlos del backend
                if (string.IsNullOrEmpty(usuarioNombre) || string.IsNullOrEmpty(usuarioRol))
                {
                    _logger.LogInformation("Obteniendo datos del usuario desde USR1");

                    var usuario = await _usuarioService.ObtenerPorEmailAsync(sesion.UsuarioID, sesion.AccessToken);

                    if (usuario != null)
                    {
                        usuarioNombre = usuario.Nombre ?? "Usuario";
                        usuarioRol = usuario.RolNombre ?? "Sin Rol";
                        usuarioRolIdStr = usuario.IdRol.ToString();

                        HttpContext.Session.SetString("UsuarioEmail", sesion.UsuarioID);
                        HttpContext.Session.SetString("UsuarioNombre", usuarioNombre);
                        HttpContext.Session.SetString("UsuarioRol", usuarioRol);
                        HttpContext.Session.SetString("UsuarioRolId", usuarioRolIdStr);

                        _logger.LogDebug("Datos guardados en sesión HTTP");
                    }
                    else
                    {
                        _logger.LogWarning("No se pudo obtener datos del usuario");
                        usuarioNombre = sesion.UsuarioID;
                        usuarioRol = "Sin Rol";
                        usuarioRolIdStr = "0";
                    }
                }

                // Poblar ViewData
                ViewData["UsuarioID"] = sesion.UsuarioID;
                ViewData["UsuarioNombre"] = usuarioNombre ?? "Usuario";
                ViewData["UsuarioRol"] = usuarioRol ?? "Invitado";
                ViewData["UsuarioRolId"] = usuarioRolIdStr ?? "0";

                _logger.LogDebug("Sesión inicializada: {Email}, Rol: {Rol} (ID: {RolId})",
                    sesion.UsuarioID, usuarioRol, usuarioRolIdStr);

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al inicializar sesión");
                return RedirectToPage("/Login");
            }
        }

        protected string ObtenerToken()
        {
            var sesion = _authService.ObtenerSesionActual();
            return sesion?.AccessToken ?? "";
        }

        protected async Task<IActionResult> CerrarSesionAsync()
        {
            try
            {
                await _authService.CerrarSesionAsync();
                HttpContext.Session.Clear();
                _logger.LogInformation("Sesión cerrada");
                return RedirectToPage("/Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar sesión");
                return RedirectToPage("/Login");
            }
        }
    }
}