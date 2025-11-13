using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;

namespace Avatar_Mod_Administración.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LoginModel> _logger;

        [BindProperty]
        public LoginRequest Input { get; set; } = new();

        public LoginModel(IAuthService authService, ILogger<LoginModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Intentar restaurar sesión desde cookies si existe
                var restaurado = await _authService.RestaurarSesionDesdeCookiesAsync();
                if (restaurado)
                {
                    _logger.LogInformation("Sesión restaurada desde cookies - redirigiendo a Dashboard");
                    return RedirectToPage("/Index");
                }

                // Verificar si ya hay sesión activa en memoria
                var sesion = _authService.ObtenerSesionActual();
                if (sesion != null)
                {
                    _logger.LogInformation("Sesión activa encontrada - redirigiendo a Dashboard");
                    return RedirectToPage("/Index");
                }

                // No hay sesión, mostrar formulario de login
                _logger.LogDebug("No hay sesión activa - mostrando formulario de login");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar sesión en Login");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _logger.LogInformation("Intentando login para: {Email}", Input.Email);

            var response = await _authService.LoginAsync(Input.Email, Input.Contrasenna);

            if (response == null)
            {
                _logger.LogWarning("Login falló - credenciales incorrectas para: {Email}", Input.Email);
                ModelState.AddModelError(string.Empty, "Usuario y/o contraseña incorrectos");
                return Page();
            }

            _logger.LogInformation("Login exitoso para: {Email}", response.UsuarioID);
            _logger.LogInformation("Recordar sesión: {Recordar}", Input.RecordarSesion ? "SÍ" : "NO");

            // Guardar sesión con cookies solo si se marcó "Recordar sesión"
            _authService.GuardarSesion(response, Input.RecordarSesion);

            return RedirectToPage("/Index");
        }
    }
}